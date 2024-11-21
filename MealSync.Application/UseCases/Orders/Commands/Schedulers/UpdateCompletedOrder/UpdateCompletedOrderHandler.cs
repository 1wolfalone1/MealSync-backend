using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.UpdateCompletedOrder;

public class UpdateCompletedOrderHandler : ICommandHandler<UpdateCompletedOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCompletedOrderHandler> _logger;
    private readonly IAccountFlagRepository _accountFlagRepository;

    public UpdateCompletedOrderHandler(
        IOrderRepository orderRepository, IPaymentRepository paymentRepository,
        IShopRepository shopRepository, IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository, IAccountRepository accountRepository,
        IBatchHistoryRepository batchHistoryRepository, IUnitOfWork unitOfWork, ILogger<UpdateCompletedOrderHandler> logger,
        INotificationFactory notificationFactory, INotifierService notifierService,
        IEmailService emailService, ISystemConfigRepository systemConfigRepository, IAccountFlagRepository accountFlagRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _shopRepository = shopRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _accountRepository = accountRepository;
        _batchHistoryRepository = batchHistoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _emailService = emailService;
        _systemConfigRepository = systemConfigRepository;
        _accountFlagRepository = accountFlagRepository;
    }

    public async Task<Result<Result>> Handle(UpdateCompletedOrderCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var (intendedReceiveDate, startTime, endTime) = TimeFrameUtils.OrderTimeFrameForBatchProcess(TimeFrameUtils.GetCurrentDateInUTC7(), 24);
        var startBatchDateTime = TimeFrameUtils.GetCurrentDate();
        var endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        _logger.LogInformation($"Update Completed Order Batch Start At: {startBatchDateTime}  (Intended Receive Date: {intendedReceiveDate} - End Time: {endTime})");

        var orders = await _orderRepository.GetFailDeliveryAndDelivered(intendedReceiveDate, endTime).ConfigureAwait(false);
        var systemConfig = _systemConfigRepository.Get().First();
        var accountsUpdateFlag = new List<Account>();
        var shopWalletUpdateAmount = new List<Wallet>();
        var shopWalletTransactionInsert = new List<WalletTransaction>();
        AccountFlag? accountFlag = default;

        foreach (var order in orders)
        {
            var payment = await _paymentRepository.GetPaymentByOrderId(order.Id).ConfigureAwait(false);
            var shop = _shopRepository.GetById(order.ShopId)!;
            var shopWallet = _walletRepository.GetById(shop.WalletId)!;
            var customerAccount = _accountRepository.GetById(order.CustomerId)!;

            if (order.Status == OrderStatus.FailDelivery)
            {
                if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                {
                    // Customer payment online => Con batch check order sau 2 giờ giao bên Thống đã chuyển tiền vô incoming và charge fee vô ví hoa hồng => Incoming to available, Flag customer => Update to completed
                    // Customer payment COD => Flag customer => Update to completed
                    if (payment.PaymentMethods != PaymentMethods.COD)
                    {
                        // => Incoming to available
                        var (wallet, walletTransactions) = await TransactionIncomingToAvailableAndUpdateOrderStatus(payment, order, shop, shopWallet).ConfigureAwait(false);
                        shopWalletUpdateAmount.Add(wallet);
                        shopWalletTransactionInsert.AddRange(walletTransactions);

                        // => Flag customer - Todo: Question
                        customerAccount.NumOfFlag += 1;
                        accountFlag = GetAccountFlagForFailDeliveryByCustomer(customerAccount, order);

                        if (customerAccount.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                        {
                            customerAccount.Status = AccountStatus.Banned;
                        }

                        accountsUpdateFlag.Add(customerAccount);

                        // => Update status to completed
                        order.Status = OrderStatus.Completed;
                    }
                    else
                    {
                        // => Flag customer - Todo: Question
                        customerAccount.NumOfFlag += 1;
                        accountFlag = GetAccountFlagForFailDeliveryByCustomer(customerAccount, order);

                        if (customerAccount.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                        {
                            customerAccount.Status = AccountStatus.Banned;
                        }

                        accountsUpdateFlag.Add(customerAccount);

                        // => Update status to completed
                        order.Status = OrderStatus.Completed;
                    }

                }
                else
                {
                    // Delivery fail by shop - Do nothing => Con batch check order sau 2 giờ giao bên Thống
                    // đã đánh cờ shop và refund nếu khách hàng thanh toán online
                    // => Only update to completed
                    order.Status = OrderStatus.Completed;
                }
            }
            else
            {
                // Customer payment online => Incoming to available => Update to completed
                // Customer payment COD => Giao hàng thành công Thống đã rút lấy tiền hoa hồng từ ví available của shop về ví tổng rồi về ví hoa hồng => Only update to completed
                if (payment.PaymentMethods != PaymentMethods.COD)
                {
                    // => Incoming to available
                    var (wallet, walletTransactions) = await TransactionIncomingToAvailableAndUpdateOrderStatus(payment, order, shop, shopWallet).ConfigureAwait(false);
                    shopWalletUpdateAmount.Add(wallet);
                    shopWalletTransactionInsert.AddRange(walletTransactions);

                    // => Update status to completed
                    order.Status = OrderStatus.Completed;
                }
                else
                {
                    // Only update to completed
                    order.Status = OrderStatus.Completed;
                }
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            _orderRepository.UpdateRange(orders);
            _walletRepository.UpdateRange(shopWalletUpdateAmount);
            await _walletTransactionRepository.AddRangeAsync(shopWalletTransactionInsert).ConfigureAwait(false);
            _accountRepository.UpdateRange(accountsUpdateFlag);
            if (accountFlag != default)
            {
                await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);
            }

            endBatchDateTime = TimeFrameUtils.GetCurrentDate();
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            errors.Add(e.Message);
        }

        foreach (var account in accountsUpdateFlag)
        {
            NotifyFlagOrBanCustomerAccount(account, systemConfig);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory
            {
                BatchCode = BatchCodes.BatchUpdateCompletedOrder,
                Parameter = string.Empty,
                TotalRecord = orders.Count,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startBatchDateTime,
                EndDateTime = endBatchDateTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            _logger.LogInformation($"Update Completed Order Batch End At: {endBatchDateTime}");
            return errors.Count > 0 ? Result.Success("Run batch fail!") : Result.Success("Run batch success!");
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private AccountFlag GetAccountFlagForFailDeliveryByCustomer(Account customerAccount, Order order)
    {
        return new AccountFlag
        {
            AccountId = customerAccount.Id,
            ActionType = AccountActionTypes.FailDeliveryByCustomerOrder,
            TargetType = AccountTargetTypes.Order,
            TargetId = order.Id.ToString(),
            Description = $"Khách hàng không nhận hàng giao từ cửa hàng đơn hàng MS-{order.Id}",
        };
    }

    private void NotifyFlagOrBanCustomerAccount(Account customerAccount, SystemConfig systemConfig)
    {
        if (customerAccount.NumOfFlag < systemConfig.MaxFlagsBeforeBan)
        {
            var notification = _notificationFactory.CreateWarningFlagCustomerNotification(customerAccount);
            _notifierService.NotifyAsync(notification);
        }
        else
        {
            _emailService.SendNotifyBannedCustomerAccount(customerAccount.Email, customerAccount.FullName, customerAccount.NumOfFlag);
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