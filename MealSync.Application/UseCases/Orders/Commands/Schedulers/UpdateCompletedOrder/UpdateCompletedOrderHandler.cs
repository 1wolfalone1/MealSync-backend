using System.Text.Json;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
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
    private readonly ICustomerRepository _customerRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IVnPayPaymentService _vnPayPaymentService;

    public UpdateCompletedOrderHandler(
        IOrderRepository orderRepository, IPaymentRepository paymentRepository,
        IShopRepository shopRepository, IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository, IAccountRepository accountRepository,
        IBatchHistoryRepository batchHistoryRepository, IUnitOfWork unitOfWork, ILogger<UpdateCompletedOrderHandler> logger,
        INotificationFactory notificationFactory, INotifierService notifierService,
        IEmailService emailService, ISystemConfigRepository systemConfigRepository, IAccountFlagRepository accountFlagRepository,
        ICustomerRepository customerRepository, IBuildingRepository buildingRepository, IVnPayPaymentService vnPayPaymentService)
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
        _customerRepository = customerRepository;
        _buildingRepository = buildingRepository;
        _vnPayPaymentService = vnPayPaymentService;
    }

    public async Task<Result<Result>> Handle(UpdateCompletedOrderCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var (intendedReceiveDate, startTime, endTime) = TimeFrameUtils.OrderTimeFrameForBatchProcess(TimeFrameUtils.GetCurrentDateInUTC7(), 12);
        var startBatchDateTime = TimeFrameUtils.GetCurrentDate();
        var endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        _logger.LogInformation($"Update Completed Order Batch Start At: {startBatchDateTime}  (Intended Receive Date: {intendedReceiveDate} - End Time: {endTime})");

        var orders = await _orderRepository.GetFailDeliveryAndDelivered(intendedReceiveDate, endTime).ConfigureAwait(false);
        var systemConfig = _systemConfigRepository.Get().First();
        var shopWalletUpdateAmount = new List<Wallet>();
        var shopWalletTransactionInsert = new List<WalletTransaction>();
        var accountFlags = new List<AccountFlag>();
        var accountsBanNotify = new List<Account>();

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            foreach (var order in orders)
            {
                var payment = await _paymentRepository.GetPaymentByOrderId(order.Id).ConfigureAwait(false);
                var shop = _shopRepository.GetById(order.ShopId)!;
                var shopWallet = _walletRepository.GetById(shop.WalletId)!;
                var account = _accountRepository.GetById(order.CustomerId)!;
                var customer = _customerRepository.GetById(order.CustomerId)!;

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
                            account.NumOfFlag += 1;
                            accountFlags.Add(GetAccountFlagForFailDeliveryByCustomer(account, order));

                            if (account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                            {
                                var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByCustomerId(account.Id).ConfigureAwait(false);
                                var ordersCancelBeforeBan = await _orderRepository.GetForSystemCancelByCustomerId(account.Id).ConfigureAwait(false);
                                if (totalOrderInProcess > 0)
                                {
                                    customer.Status = CustomerStatus.Banning;
                                }
                                else
                                {
                                    customer.Status = CustomerStatus.Banned;
                                    account.Status = AccountStatus.Banned;
                                }

                                accountsBanNotify.Add(account);
                                _customerRepository.Update(customer);
                                await CancelOrderPendingOrConfirmedForBanCustomer(ordersCancelBeforeBan).ConfigureAwait(false);
                            }

                            // => Update status to completed
                            order.Status = OrderStatus.Completed;
                        }
                        else
                        {
                            // => Update status to completed
                            order.Status = OrderStatus.Completed;
                        }

                        _accountRepository.Update(account);

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

            _orderRepository.UpdateRange(orders);
            _walletRepository.UpdateRange(shopWalletUpdateAmount);
            await _walletTransactionRepository.AddRangeAsync(shopWalletTransactionInsert).ConfigureAwait(false);
            if (accountFlags.Count > 0)
            {
                await _accountFlagRepository.AddRangeAsync(accountFlags).ConfigureAwait(false);
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

        foreach (var account in accountsBanNotify)
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

    private async Task CancelOrderPendingOrConfirmedForBanCustomer(List<Order> ordersCancelBeforeBan)
    {
        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription();
            _orderRepository.Update(order);
            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
                await RefundOrderAsync(order, payment).ConfigureAwait(false);
            }
        }
    }

    private async Task<bool> RefundOrderAsync(Order order, Payment payment)
    {
        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
        {
            // Refund + update status order to cancel
            var refundPayment = new Payment
            {
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = PaymentStatus.Pending,
                Type = PaymentTypes.Refund,
                PaymentMethods = PaymentMethods.VnPay,
            };
            var refundResult = await _vnPayPaymentService.CreateRefund(payment).ConfigureAwait(false);
            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
            var content = JsonSerializer.Serialize(refundResult, options);

            refundPayment.PaymentThirdPartyId = refundResult.VnpTransactionNo;
            refundPayment.PaymentThirdPartyContent = content;

            if (refundResult.VnpResponseCode == ((int)VnPayRefundResponseCode.CODE_00).ToString("D2"))
            {
                refundPayment.Status = PaymentStatus.PaidSuccess;

                // Rút tiền từ ví hoa hồng về ví hệ thống sau đó refund tiền về cho customer
                var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);
                var listWalletTransaction = new List<WalletTransaction>();

                WalletTransaction transactionWithdrawalSystemCommissionToSystemTotal = new WalletTransaction
                {
                    WalletFromId = systemCommissionWallet.Id,
                    WalletToId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                    IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                    ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
                    Amount = -order.ChargeFee,
                    Type = WalletTransactionType.Withdrawal,
                    Description = $"Rút tiền từ ví hoa hồng {MoneyUtils.FormatMoneyWithDots(order.ChargeFee)} VNĐ về ví tổng hệ thống",
                };

                systemCommissionWallet.AvailableAmount -= order.ChargeFee;
                listWalletTransaction.Add(transactionWithdrawalSystemCommissionToSystemTotal);

                WalletTransaction transactionAddFromSystemCommissionToSystemTotal = new WalletTransaction
                {
                    WalletFromId = systemCommissionWallet.Id,
                    WalletToId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                    IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                    ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                    Amount = order.ChargeFee,
                    Type = WalletTransactionType.Transfer,
                    Description = $"Tiền từ ví hoa hồng chuyển về ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(order.ChargeFee)} VNĐ",
                };
                systemTotalWallet.AvailableAmount += order.ChargeFee;
                listWalletTransaction.Add(transactionAddFromSystemCommissionToSystemTotal);

                WalletTransaction transactionWithdrawalSystemTotalForRefundPaymentOnline = new WalletTransaction
                {
                    WalletFromId = systemTotalWallet.Id,
                    AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
                    IncomingAmountBefore = systemTotalWallet.IncomingAmount,
                    ReportingAmountBefore = systemTotalWallet.ReportingAmount,
                    Amount = -payment.Amount,
                    Type = WalletTransactionType.Withdrawal,
                    Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(payment.Amount)} VNĐ để hoàn tiền giao dịch thanh toán online của đơn hàng MS-{payment.OrderId}",
                };
                systemTotalWallet.AvailableAmount -= payment.Amount;
                listWalletTransaction.Add(transactionWithdrawalSystemTotalForRefundPaymentOnline);

                refundPayment.WalletTransactions = listWalletTransaction;
                _walletRepository.Update(systemTotalWallet);
                _walletRepository.Update(systemCommissionWallet);
            }
            else
            {
                refundPayment.Status = PaymentStatus.PaidFail;

                // Get moderator account to send mail
                await SendEmailAnnounceModeratorAsync(order).ConfigureAwait(false);

                // Send notification for moderator
                NotifyAnnounceRefundFailAsync(order);
            }

            await _paymentRepository.AddAsync(refundPayment).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    private async Task SendEmailAnnounceModeratorAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building!.DormitoryId);
        if (moderators != default && moderators.Count > 0)
        {
            foreach (var moderator in moderators)
            {
                _emailService.SendEmailToAnnounceModeratorRefundFail(moderator.Email, order.Id);
            }
        }
    }

    private void NotifyAnnounceRefundFailAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building!.DormitoryId);
        if (moderators != default && moderators.Count > 0)
        {
            foreach (var moderator in moderators)
            {
                var notification = _notificationFactory.CreateRefundFaillNotification(order, moderator);
                _notifierService.NotifyAsync(notification);
            }
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