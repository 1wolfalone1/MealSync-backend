using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess;

public class ShopAndStaffDeliverySuccessHandler : ICommandHandler<ShopAndStaffDeliverySuccessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopAndStaffDeliverySuccessHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IAccountRepository _accountRepository;
    private readonly IConfiguration _configuration;
    private readonly IWalletRepository _walletRepository;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private const string QR_KEY = "QR_KEY";

    public ShopAndStaffDeliverySuccessHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository, ILogger<ShopAndStaffDeliverySuccessHandler> logger,
        ISystemResourceRepository systemResourceRepository, IShopRepository shopRepository, IOrderDetailRepository orderDetailRepository, IFoodRepository foodRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, IAccountRepository accountRepository, IConfiguration configuration, IWalletRepository walletRepository, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
        _shopRepository = shopRepository;
        _orderDetailRepository = orderDetailRepository;
        _foodRepository = foodRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _accountRepository = accountRepository;
        _configuration = configuration;
        _walletRepository = walletRepository;
        _currentAccountService = currentAccountService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<Result>> Handle(ShopAndStaffDeliverySuccessCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        if (!request.IsConfirm)
        {
            var order = _orderRepository.GetById(request.OrderId);
            if (order.DeliveryPackage.ShopDeliveryStaffId.HasValue && order.DeliveryPackage.ShopDeliveryStaffId == request.ShipperId || order.DeliveryPackage.ShopId.HasValue && order.DeliveryPackage.ShopId == request.ShipperId)
            {
                return Result.Warning(new
                {
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_NOT_DELIVERY_BY_YOU.GetDescription(), order.Id),
                    Code = MessageCode.W_ORDER_NOT_DELIVERY_BY_YOU.GetDescription(),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.Get(o => o.Id == request.OrderId)
                .Include(o => o.Payments)
                .Include(o => o.DeliveryPackage).Single();
            order.Status = OrderStatus.Delivered;

            // Increase food total order
            var listFoodIds = _orderDetailRepository.GetListFoodIdInOrderDetailGroupBy(request.OrderId);
            var listFood = _foodRepository.Get(f => listFoodIds.Contains(f.Id)).ToList();
            foreach (var food in listFood)
            {
                food.TotalOrder++;
            }

            // Increase shop total order
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            shop.TotalOrder++;

            // Increase money shop wallet
            var amountIncrease = order.TotalPrice - order.ChargeFee - order.TotalPromotion;
            var payment = order.Payments.Where(p => p.Type == PaymentTypes.Payment).First();
            await CreateTransactionToShopAsync(payment.Id, order.Id, amountIncrease).ConfigureAwait(false);

            _foodRepository.UpdateRange(listFood);
            _orderRepository.Update(order);
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Noti to customer
            var notiCustomer = _notificationFactory.CreateOrderCustomerDeliveredNotification(order, shop);
            _notifierService.NotifyAsync(notiCustomer);

            // Noti to shop
            Account accShip;
            if (order.DeliveryPackage.ShopDeliveryStaffId != null)
            {
                accShip = _accountRepository.GetById(order.DeliveryPackage.ShopDeliveryStaffId);
            }
            else
            {
                accShip = _accountRepository.GetById(order.ShopId);
            }

            var notiShop = _notificationFactory.CreateOrderShopDeliveredNotification(order, accShip);
            _notifierService.NotifyAsync(notiShop);
            return Result.Success(new
            {
                Code = MessageCode.I_ORDER_DELIVERD.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_DELIVERD.GetDescription(), new object[] { request.OrderId }),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task CreateTransactionToShopAsync(long paymentId, long orderId, double amountSendToShop)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var shop = _shopRepository.Get(sh => sh.Id == shopId)
            .Include(sh => sh.Wallet).Single();

        List<WalletTransaction> transactionsAdds = new();
        WalletTransaction transactionWithdrawalSystemTotalToShopWallet = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
            IncomingAmountBefore = systemTotalWallet.IncomingAmount,
            ReportingAmountBefore = systemTotalWallet.ReportingAmount,
            Amount = -amountSendToShop,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ về ví shop id {shopId} từ đơn hàng MS-{orderId}",
        };
        transactionsAdds.Add(transactionWithdrawalSystemTotalToShopWallet);

        WalletTransaction transactionAddFromSystemTotalToShop = new WalletTransaction
        {
            WalletFromId = systemTotalWallet.Id,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shop.Wallet.AvailableAmount,
            IncomingAmountBefore = shop.Wallet.IncomingAmount,
            ReportingAmountBefore = shop.Wallet.ReportingAmount,
            Amount = amountSendToShop,
            Type = WalletTransactionType.Transfer,
            PaymentId = paymentId,
            Description = $"Tiền thanh toán cho đơn hàng MS-{orderId} {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ",
        };
        transactionsAdds.Add(transactionAddFromSystemTotalToShop);

        List<Wallet> wallets = new();
        systemTotalWallet.AvailableAmount -= amountSendToShop;
        shop.Wallet.IncomingAmount += amountSendToShop;
        wallets.Add(systemTotalWallet);
        wallets.Add(shop.Wallet);

        _walletRepository.UpdateRange(wallets);
        await _walletTransactionRepository.AddRangeAsync(transactionsAdds).ConfigureAwait(false);
    }

    private void Validate(ShopAndStaffDeliverySuccessCommand request)
    {
        // Todo: Add check out of frame and in wrong date

        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == shopId)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Delivering)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.DeliveryPackage != null)
        {
            if (order.CustomerId != request.CustomerId)
            {
                // Đơn hàng không phải của khách hàng
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_CORRECT_CUSTOMER.GetDescription(), new object[] { request.OrderId, order.FullName });
            }
        }

        // Check token gen is correct
        var originString = order.Id + order.DeliveryPackageId + order.CustomerId + order.PhoneNumber + order.FullName + _configuration[QR_KEY];
        if (!BCrypUnitls.Verify(originString, request.Token))
            throw new InvalidBusinessException(MessageCode.E_ORDER_QR_SCAN_NOT_CORRECT.GetDescription());
    }
}