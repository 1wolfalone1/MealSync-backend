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
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.CompleteOrder;

public class CompleteOrderHandler : ICommandHandler<CompleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CompleteOrderHandler> _logger;
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public CompleteOrderHandler(
        IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService,
        ISystemResourceRepository systemResourceRepository, IUnitOfWork unitOfWork,
        INotificationFactory notificationFactory, INotificationService notificationService,
        ILogger<CompleteOrderHandler> logger, IAccountRepository accountRepository,
        IWalletRepository walletRepository, IShopRepository shopRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _notificationFactory = notificationFactory;
        _notificationService = notificationService;
        _logger = logger;
        _accountRepository = accountRepository;
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<Result>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = await _orderRepository.GetByIdAndCustomerIdIncludePayment(request.Id, customerId).ConfigureAwait(false);
        if (order == default)
        {
            // Throw exception order not found
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else if (order.Status != OrderStatus.Delivered)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_STATUS_FOR_COMPLETED.GetDescription());
        }
        else if (!request.IsConfirm)
        {
            return Result.Warning(new
            {
                Code = MessageCode.W_ORDER_CONFIRM_ORDER_COMPLETED.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_CONFIRM_ORDER_COMPLETED.GetDescription(), new object[] { request.Id }),
            });
        }
        else
        {
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                // Customer payment online => Incoming to available => Update to completed
                // Customer payment COD => Giao hàng thành công Thống đã rút lấy tiền hoa hồng từ ví available của shop về ví tổng rồi về ví hoa hồng => Only update to completed
                var payment = order.Payments.First(p => p.Type == PaymentTypes.Payment);
                if (payment.PaymentMethods != PaymentMethods.COD)
                {
                    // Incoming to available
                    var shop = _shopRepository.GetById(order.ShopId)!;
                    var shopWallet = _walletRepository.GetById(shop.WalletId)!;
                    var (wallet, walletTransactions) = await TransactionIncomingToAvailableAndUpdateOrderStatus(payment, order, shop, shopWallet).ConfigureAwait(false);
                    _walletRepository.Update(wallet);
                    await _walletTransactionRepository.AddRangeAsync(walletTransactions).ConfigureAwait(false);
                }
                else
                {
                    // Do nothing: Only update completed
                }

                order.Status = OrderStatus.Completed;
                order.ReasonIdentity = null;
                _orderRepository.Update(order);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                var account = _accountRepository.GetById(order.CustomerId)!;
                var notification = _notificationFactory.CreateCustomerCompletedOrderNotification(order, account);
                await _notificationService.NotifyAsync(notification).ConfigureAwait(false);

                return Result.Success(new
                {
                    Code = MessageCode.I_ORDER_CONFIRM_ORDER_COMPLETED.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_CONFIRM_ORDER_COMPLETED.GetDescription(), new object[] { request.Id }),
                });
            }
            catch (Exception e)
            {
                // Rollback when exception
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
    }

    private async Task<(Wallet ShopWallet, List<WalletTransaction> WalletTransactions)> TransactionIncomingToAvailableAndUpdateOrderStatus(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {
        List<WalletTransaction> walletTransactions = new List<WalletTransaction>();

        // Incoming to available
        var incomingAmountOrder = payment.Amount - order.ChargeFee;
        var transactionWithdrawalIncomingAmountToAvailableAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -incomingAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền chờ về {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ sang tiền có sẵn",
        };

        shopWallet.IncomingAmount -= incomingAmountOrder;

        var transactionAddFromIncomingAmountToAvailableAmountOfShop = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = shop.WalletId,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = incomingAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền chờ về cộng vào {MoneyUtils.FormatMoneyWithDots(incomingAmountOrder)} VNĐ tiền có sẵn",
        };

        shopWallet.AvailableAmount += incomingAmountOrder;

        walletTransactions.Add(transactionWithdrawalIncomingAmountToAvailableAmountOfShop);
        walletTransactions.Add(transactionAddFromIncomingAmountToAvailableAmountOfShop);

        return (shopWallet, walletTransactions);
    }
}