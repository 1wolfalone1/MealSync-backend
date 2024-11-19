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
    private readonly IEmailService _emailService;
    private readonly IPaymentRepository _paymentRepository;
    private const string QR_KEY = "QR_KEY";

    public ShopAndStaffDeliverySuccessHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository, ILogger<ShopAndStaffDeliverySuccessHandler> logger,
        ISystemResourceRepository systemResourceRepository, IShopRepository shopRepository, IOrderDetailRepository orderDetailRepository, IFoodRepository foodRepository, INotificationFactory notificationFactory,
        INotifierService notifierService, IAccountRepository accountRepository, IConfiguration configuration, IWalletRepository walletRepository, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IWalletTransactionRepository walletTransactionRepository, IEmailService emailService, IPaymentRepository paymentRepository)
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
        _emailService = emailService;
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<Result>> Handle(ShopAndStaffDeliverySuccessCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var order = _orderRepository.Get(o => o.Id == request.OrderId)
            .Include(o => o.Payments)
            .Include(o => o.DeliveryPackage).Single();
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var shop = _shopRepository.GetById(shopId);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            order.Status = OrderStatus.Delivered;
            order.ReceiveAt = TimeFrameUtils.GetCurrentDate();

            // Increase food total order
            var listFoodIds = _orderDetailRepository.GetListFoodIdInOrderDetailGroupBy(request.OrderId);
            var listFood = _foodRepository.Get(f => listFoodIds.Contains(f.Id)).ToList();
            foreach (var food in listFood)
            {
                food.TotalOrder++;
            }

            // Increase shop total order
            shop.TotalOrder++;

            // Increase money shop wallet
            var amountIncrease = order.TotalPrice - order.ChargeFee - order.TotalPromotion;
            var payment = order.Payments.Where(p => p.Type == PaymentTypes.Payment).First();
            var isPaidToShop = await CreateTransactionToShopAsync(payment, order.Id, amountIncrease, order.ChargeFee).ConfigureAwait(false);
            order.IsPaidToShop = isPaidToShop;

            _foodRepository.UpdateRange(listFood);
            _orderRepository.Update(order);
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

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

    private async Task<bool> CreateTransactionToShopAsync(Payment payment, long orderId, double amountSendToShop, double chargeFee)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var shop = _shopRepository.Get(sh => sh.Id == shopId)
            .Include(sh => sh.Wallet).Single();

        // If customer pay by vnpay will take form system give shop wallet
        if (payment.PaymentMethods == PaymentMethods.VnPay)
        {
            List<WalletTransaction> transactionsAdds = new();
            WalletTransaction transactionWithdrawalSystemTotalToShopWallet = new WalletTransaction
            {
                WalletFromId = systemTotalWallet.Id,
                WalletToId = shop.WalletId,
                AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                Amount = -amountSendToShop,
                PaymentId = payment.Id,
                Type = WalletTransactionType.Withdrawal,
                Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ về ví cửa hàng id {shopId} từ đơn hàng MS-{orderId}",
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
                PaymentId = payment.Id,
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
            var order = _orderRepository.GetById(orderId);
            var noti = _notificationFactory.CreateShopWalletReceiveIncommingAmountNotification(order, shop.Account, amountSendToShop);
            _notifierService.NotifyAsync(noti);

            return true;
        }

        // COD need to take commission fee from available wallet
        else if (payment.PaymentMethods == PaymentMethods.COD)
        {
            var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);
            var transactionWithdrawalAvailableAmountOfShop = new WalletTransaction
            {
                WalletFromId = shop.WalletId,
                WalletToId = systemTotalWallet.Id,
                AvaiableAmountBefore = shop.Wallet.AvailableAmount,
                IncomingAmountBefore = shop.Wallet.IncomingAmount,
                ReportingAmountBefore = shop.Wallet.ReportingAmount,
                Amount = -chargeFee,
                Type = WalletTransactionType.Withdrawal,
                Description = $"Rút tiền hoa hồng từ tiền có sẵn {MoneyUtils.FormatMoneyWithDots(chargeFee)} VNĐ của đơn hàng MS-{orderId} về ví hoa hồng",
            };
            shop.Wallet.AvailableAmount -= chargeFee;

            var transactionAddFromAvailableShopToCommissionWallet = new WalletTransaction
            {
                WalletFromId = shop.WalletId,
                WalletToId = systemCommissionWallet.Id,
                AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
                Amount = chargeFee,
                Type = WalletTransactionType.Transfer,
                Description = $"Tiền hoa hồng từ đơn hàng MS-{orderId} {MoneyUtils.FormatMoneyWithDots(chargeFee)} VNĐ về ví hoa hồng",
            };
            systemCommissionWallet.AvailableAmount += chargeFee;
            payment.Status = PaymentStatus.PaidSuccess;

            _walletRepository.Update(shop.Wallet);
            _walletRepository.Update(systemTotalWallet);
            _paymentRepository.Update(payment);

            await _walletTransactionRepository.AddAsync(transactionWithdrawalAvailableAmountOfShop).ConfigureAwait(false);
            await _walletTransactionRepository.AddAsync(transactionAddFromAvailableShopToCommissionWallet).ConfigureAwait(false);

            // BR: Tiền có sẵn < -200000 => shop inactive không được bán nữa cho tới khi nạp tiền vào
            if (shop.Wallet.AvailableAmount < MoneyUtils.AVAILABLE_AMOUNT_LIMIT)
            {
                var notification = _notificationFactory.CreateLimitAvailableAmountAndInActiveShopNotification(shop, shop.Wallet);
                _notifierService.NotifyAsync(notification);
                _emailService.SendNotifyLimitAvailableAmountAndInActiveShop(
                    account.Email,
                    MoneyUtils.FormatMoneyWithDots(shop.Wallet.AvailableAmount),
                    MoneyUtils.FormatMoneyWithDots(MoneyUtils.AVAILABLE_AMOUNT_LIMIT));
                shop.Status = ShopStatus.InActive;
                _shopRepository.Update(shop);
            }

            var order = _orderRepository.GetById(orderId);
            var accountShop = _accountRepository.GetById(shop.Id);
            var noti = _notificationFactory.CreateTakeCommissionFromShopWalletNotification(order, accountShop, chargeFee);
            _notifierService.NotifyAsync(noti);
        }

        return false;
    }

    private void Validate(ShopAndStaffDeliverySuccessCommand request)
    {
        if (request.OrderId != request.OrderRequestId)
            throw new InvalidBusinessException(MessageCode.E_ORDER_QR_SCAN_NOT_CORRECT.GetDescription());

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

            if (order.DeliveryPackage.ShopDeliveryStaffId.HasValue && order.DeliveryPackage.ShopDeliveryStaffId != request.ShipperId && order.DeliveryPackage.ShopId.HasValue && order.DeliveryPackage.ShopId != request.ShipperId)
            {
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERY_BY_YOU.GetDescription(), new object[] { request.OrderId });
            }
        }

        var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
        var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);

        if (currentDateTime.DateTime < startEndTime.StartTime || currentDateTime.DateTime > startEndTime.EndTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME_TO_DELIVERED.GetDescription(), new object[] { request.OrderId, TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime) });

        // Check token gen is correct
        var originString = order.Id + order.DeliveryPackageId + order.CustomerId + order.PhoneNumber + order.FullName + _configuration[QR_KEY];
        if (!BCrypUnitls.Verify(originString, request.Token))
            throw new InvalidBusinessException(MessageCode.E_ORDER_QR_SCAN_NOT_CORRECT.GetDescription());
    }
}