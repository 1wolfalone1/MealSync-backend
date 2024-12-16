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

namespace MealSync.Application.UseCases.Reports.Commands.Schedulers.ApproveCustomerReportFailDeliveryByShop;

public class ApproveCustomerReportFailDeliveryByShopHandler : ICommandHandler<ApproveCustomerReportFailDeliveryByShopCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IVnPayPaymentService _vnPayPaymentService;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly ILogger<ApproveCustomerReportFailDeliveryByShopHandler> _logger;

    public ApproveCustomerReportFailDeliveryByShopHandler(
        IOrderRepository orderRepository, IReportRepository reportRepository, IAccountRepository accountRepository,
        IShopRepository shopRepository, IVnPayPaymentService vnPayPaymentService, IWalletRepository walletRepository,
        IPaymentRepository paymentRepository, IBuildingRepository buildingRepository, IEmailService emailService,
        INotificationFactory notificationFactory, INotifierService notifierService, ISystemConfigRepository systemConfigRepository,
        IWalletTransactionRepository walletTransactionRepository, ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork, IBatchHistoryRepository batchHistoryRepository, ILogger<ApproveCustomerReportFailDeliveryByShopHandler> logger
    )
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
        _vnPayPaymentService = vnPayPaymentService;
        _walletRepository = walletRepository;
        _paymentRepository = paymentRepository;
        _buildingRepository = buildingRepository;
        _emailService = emailService;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _systemConfigRepository = systemConfigRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _batchHistoryRepository = batchHistoryRepository;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(ApproveCustomerReportFailDeliveryByShopCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var (intendedReceiveDate, startTime, endTime) = TimeFrameUtils.OrderTimeFrameForBatchProcess(TimeFrameUtils.GetCurrentDateInUTC7(), 2);
        var startBatchDateTime = TimeFrameUtils.GetCurrentDate();
        var endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        _logger.LogInformation($"Approve Customer Report Fail Delivery By Shop Batch Start At: {startBatchDateTime}  (Intended Receive Date: {intendedReceiveDate} - End Time: {endTime})");

        var orders = await _orderRepository.GetReportOrderFailByShop(intendedReceiveDate, endTime).ConfigureAwait(false);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var order in orders)
            {
                var reports = await _reportRepository.GetByOrderId(order.Id).ConfigureAwait(false);
                var customerReport = reports.First(r => r.CustomerId != default);
                var shopReport = reports.Count > 1 ? reports.First(r => r.ShopId != default) : default;
                var shop = _shopRepository.GetById(order.ShopId)!;
                var shopAccount = _accountRepository.GetById(order.ShopId)!;
                var customer = await _customerRepository.GetIncludeAccount(order.CustomerId).ConfigureAwait(false);
                var payment = order.Payments.First(p => p.Type == PaymentTypes.Payment);
                var systemConfig = _systemConfigRepository.GetSystemConfig();
                var shopWallet = _walletRepository.GetById(shop.WalletId)!;

                // Fail delivery
                if (payment.PaymentMethods == PaymentMethods.COD)
                {
                    // Giao hàng thất bại, thanh toán COD => Approve customer report => Đánh cờ shop
                    await ApproveCustomerAndFlagShop(customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                }
                else
                {
                    // Giao hàng thất bại, thanh toán Online => Approve customer report, đánh cờ shop => Refund tiền customer
                    await ApproveCustomerAndFlagShop(customerReport, shopReport, shop, systemConfig, shopAccount).ConfigureAwait(false);
                    await TransactionWithdrawalReportingForRefund(payment, order, shop, shopWallet).ConfigureAwait(false);
                    order.IsRefund = await RefundOrderAsync(order, payment).ConfigureAwait(false);
                }

                // Resolved Order
                order.Status = OrderStatus.Resolved;
                _orderRepository.Update(order);

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                NotifyApproveOrReject(customer.Account, shop, customerReport);
            }
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory
            {
                BatchCode = BatchCodes.BatchApproveCustomerReportFailDeliveryByShop,
                Parameter = string.Empty,
                TotalRecord = orders.Count,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startBatchDateTime,
                EndDateTime = endBatchDateTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            _logger.LogInformation($"Approve Customer Report Fail Delivery By Shop Batch End At: {endBatchDateTime}");
            return errors.Count > 0 ? Result.Success("Run batch fail!") : Result.Success("Run batch success!");
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void NotifyApproveOrReject(Account customerAccount, Shop shop, Report customerReport)
    {
        if (customerReport.Status == ReportStatus.Approved)
        {
            var customerNotify = _notificationFactory.CreateApproveOrRejectCustomerReportNotification(customerAccount, shop, customerReport, true);
            var shopNotify = _notificationFactory.CreateApproveOrRejectReportOfShopNotification(shop, customerAccount, customerReport, false);
            _notifierService.NotifyAsync(customerNotify);
            _notifierService.NotifyAsync(shopNotify);
        }
        else
        {
            var customerNotify = _notificationFactory.CreateApproveOrRejectCustomerReportNotification(customerAccount, shop, customerReport, false);
            var shopNotify = _notificationFactory.CreateApproveOrRejectReportOfShopNotification(shop, customerAccount, customerReport, true);
            _notifierService.NotifyAsync(customerNotify);
            _notifierService.NotifyAsync(shopNotify);
        }
    }

    private async Task TransactionWithdrawalReportingForRefund(Payment payment, Order order, Shop shop, Wallet shopWallet)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var reportAmountOrder = payment.Amount - order.ChargeFee;
        var transactions = new List<WalletTransaction>();

        var transactionWithdrawalReportingForRefund = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = systemTotalWallet.Id,
            AvaiableAmountBefore = shopWallet.AvailableAmount,
            IncomingAmountBefore = shopWallet.IncomingAmount,
            ReportingAmountBefore = shopWallet.ReportingAmount,
            Amount = -reportAmountOrder,
            Type = WalletTransactionType.Withdrawal,
            Description = $"Rút tiền từ tiền đang bị báo cáo {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ về ví tổng hệ thống để hoàn tiền cho khách hàng đơn hàng MS-{order.Id}",
        };
        transactions.Add(transactionWithdrawalReportingForRefund);
        shopWallet.ReportingAmount -= reportAmountOrder;

        var transactionTransferReportingForRefund = new WalletTransaction
        {
            WalletFromId = shop.WalletId,
            WalletToId = systemTotalWallet.Id,
            AvaiableAmountBefore = systemTotalWallet.AvailableAmount,
            IncomingAmountBefore = systemTotalWallet.IncomingAmount,
            ReportingAmountBefore = systemTotalWallet.ReportingAmount,
            Amount = -reportAmountOrder,
            Type = WalletTransactionType.Transfer,
            Description = $"Tiền từ tiền đang bị báo cáo {MoneyUtils.FormatMoneyWithDots(reportAmountOrder)} VNĐ về ví tổng hệ thống để hoàn tiền cho khách hàng đơn hàng MS-{order.Id}",
        };
        transactions.Add(transactionTransferReportingForRefund);
        systemTotalWallet.AvailableAmount += reportAmountOrder;

        await _walletTransactionRepository.AddRangeAsync(transactions).ConfigureAwait(false);
        _walletRepository.Update(shopWallet);
        _walletRepository.Update(systemTotalWallet);
    }

    private async Task ApproveCustomerAndFlagShop(Report customerReport, Report? shopReport, Shop shop, SystemConfig systemConfig, Account shopAccount)
    {
        var reason = "Lỗi do cửa hàng không giao hàng";
        customerReport.Status = ReportStatus.Approved;
        customerReport.Reason = reason;
        _reportRepository.Update(customerReport);
        if (shopReport != default)
        {
            shopReport.Status = ReportStatus.Rejected;
            shopReport.Reason = reason;
            _reportRepository.Update(shopReport);
        }

        shop.NumOfWarning += 1;

        if (shop.NumOfWarning >= 3 && shop.NumOfWarning < systemConfig.MaxWarningBeforeInscreaseFlag)
        {
            // Cờ cảnh báo thứ 3 hoặc 4 sẽ send mail thông báo
        }
        else if (shop.NumOfWarning >= systemConfig.MaxWarningBeforeInscreaseFlag)
        {
            // Cờ cảnh cáo thứ 5 => tăng cờ ở account, reset cờ cảnh báo
            shopAccount.NumOfFlag += 1;
            shop.NumOfWarning = 0;
            if (shopAccount.NumOfFlag < systemConfig.MaxFlagsBeforeBan)
            {
                // Cờ ở account < 3 => email notify warning
            }
            else
            {
                // Cờ ở account = 3 => email notify ban + ban account(Check banning, banned)
                var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByShopId(shop.Id).ConfigureAwait(false);
                if (totalOrderInProcess > 0)
                {
                    shop.Status = ShopStatus.Banning;
                }
                else
                {
                    shop.Status = ShopStatus.Banned;
                    shopAccount.Status = AccountStatus.Banned;
                }

                var ordersCancel = await _orderRepository.GetForSystemCancelByShopId(shop.Id).ConfigureAwait(false);
                await CancelOrderPendingOrConfirmedForBanShop(ordersCancel).ConfigureAwait(false);

                // Update account flag and status
                _accountRepository.Update(shopAccount);
            }
        }

        // Update shop warning and status
        _shopRepository.Update(shop);
    }

    private async Task CancelOrderPendingOrConfirmedForBanShop(List<Order> ordersCancelBeforeBan)
    {
        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription();
            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
                order.IsRefund = await RefundOrderAsync(order, payment).ConfigureAwait(false);
            }

            _orderRepository.Update(order);
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
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
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
}