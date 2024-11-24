using System.Text.Json;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.TransferIncomingAmountForShopAfterTwoHour;

public class TransferIncomingAmountForShopAfterTwoHourHandler : ICommandHandler<TransferIncomingAmountForShopAfterTwoHourCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransferIncomingAmountForShopAfterTwoHourHandler> _logger;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEmailService _emailService;

    public TransferIncomingAmountForShopAfterTwoHourHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<TransferIncomingAmountForShopAfterTwoHourHandler> logger, INotifierService notifierService, INotificationFactory notificationFactory, IBatchHistoryRepository batchHistoryRepository, IWalletRepository walletRepository, IShopRepository shopRepository, IPaymentRepository paymentRepository, IWalletTransactionRepository walletTransactionRepository, IVnPayPaymentService paymentService, IAccountRepository accountRepository, IBuildingRepository buildingRepository, IEmailService emailService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _batchHistoryRepository = batchHistoryRepository;
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _paymentRepository = paymentRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _paymentService = paymentService;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _emailService = emailService;
    }

    public async Task<Result<Result>> Handle(TransferIncomingAmountForShopAfterTwoHourCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var listOrderOverTimeFrame = _orderRepository.GetListOrderOnStatusFailDeliveredWithoutPayIncomingShop(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON, TimeFrameUtils.GetCurrentDateInUTC7().DateTime);
            totalRecord = listOrderOverTimeFrame != null ? listOrderOverTimeFrame.Count : 0;
            var notifications = new List<Notification>();
            var ordersUpdate = new List<Order>();
            var orderGroupByShopId = listOrderOverTimeFrame.GroupBy(o => o.ShopId);
            foreach (var shopOrder in orderGroupByShopId)
            {
                var marlDeliveryFail = await TransferInComingAmountAndRefundOfShopWallet(shopOrder.ToList(), shopOrder.Key).ConfigureAwait(false);
                ordersUpdate.AddRange(marlDeliveryFail.Orders);
                notifications.AddRange(marlDeliveryFail.Notifications);
            }

            _notifierService.NotifyRangeAsync(notifications);
            _orderRepository.UpdateRange(ordersUpdate);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            endTime = TimeFrameUtils.GetCurrentDate();
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            errors.Add(e.Message);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory()
            {
                BatchCode = BatchCodes.BatchCheduleTransferMoneyToShopWallet,
                Parameter = string.Empty,
                TotalRecord = totalRecord,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startTime,
                EndDateTime = endTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(batchHistory);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<(List<Order> Orders, List<Notification> Notifications)> TransferInComingAmountAndRefundOfShopWallet(List<Order> orders, long shopId)
    {
        var notifications = new List<Notification>();
        foreach (var order in orders)
        {
            if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
            {
                notifications.Add(await CreateTransferTransactionAsync(order).ConfigureAwait(false));
                order.IsPaidToShop = true;
            }
            else if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription())
            {
                var notiAndRefund = await CreateRefundPaymentCustomerAsync(order).ConfigureAwait(false);
                notifications.AddRange(notiAndRefund.Noti);
                order.IsRefund = notiAndRefund.IsRefund;
            }
        }

        return (orders, notifications);
    }

    private async Task<Notification> CreateTransferTransactionAsync(Order order)
    {
        var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
        var shop = _shopRepository.Get(sh => sh.Id == order.ShopId)
            .Include(sh => sh.Account)
            .Include(sh => sh.Wallet).Single();
        var payment = await _paymentRepository.GetPaymentVnPayByOrderId(order.Id).ConfigureAwait(false);

        var amountSendToShop = order.TotalPrice - order.TotalPromotion - order.ChargeFee;
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
            Description = $"Rút tiền từ ví tổng hệ thống {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ về ví chờ cửa hàng id {order.ShopId} từ đơn hàng MS-{order.Id}",
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
            Description = $"Tiền thanh toán cho đơn hàng MS-{order.Id} {MoneyUtils.FormatMoneyWithDots(amountSendToShop)} VNĐ về ví chờ",
        };
        transactionsAdds.Add(transactionAddFromSystemTotalToShop);

        List<Wallet> wallets = new();
        systemTotalWallet.AvailableAmount -= amountSendToShop;
        shop.Wallet.IncomingAmount += amountSendToShop;
        wallets.Add(systemTotalWallet);
        wallets.Add(shop.Wallet);
        _walletRepository.UpdateRange(wallets);
        await _walletTransactionRepository.AddRangeAsync(transactionsAdds).ConfigureAwait(false);
        var noti = _notificationFactory.CreateShopWalletReceiveIncommingAmountNotification(order, shop.Account, amountSendToShop);
        return noti;
    }

    private async Task<(List<Notification> Noti, bool IsRefund)> CreateRefundPaymentCustomerAsync(Order order)
    {
        var notifications = new List<Notification>();
        bool isRefund = false;
        var payment = await _paymentRepository.GetPaymentVnPayByOrderId(order.Id).ConfigureAwait(false);

        if (payment.Status == PaymentStatus.PaidSuccess)
        {
            // Refund + update status order to cancel
            var refundPayment = new Payment
            {
                OrderId = order.Id,
                Amount = payment.Amount,
                Status = PaymentStatus.Pending,
                Type = PaymentTypes.Refund,
                PaymentMethods = PaymentMethods.BankTransfer,
            };
            var refundResult = await _paymentService.CreateRefund(payment).ConfigureAwait(false);
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
                listWalletTransaction.Add(transactionWithdrawalSystemCommissionToSystemTotal);
                systemCommissionWallet.AvailableAmount -= order.ChargeFee;

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
                listWalletTransaction.Add(transactionAddFromSystemCommissionToSystemTotal);
                systemTotalWallet.AvailableAmount += order.ChargeFee;

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
                listWalletTransaction.Add(transactionWithdrawalSystemTotalForRefundPaymentOnline);
                systemTotalWallet.AvailableAmount -= payment.Amount;

                // await _walletTransactionRepository.AddRangeAsync(listWalletTransaction).ConfigureAwait(false);
                refundPayment.WalletTransactions = listWalletTransaction;
                _walletRepository.Update(systemTotalWallet);
                _walletRepository.Update(systemCommissionWallet);
                var shopAccount = _accountRepository.GetById(order.ShopId);
                var noti = _notificationFactory.CreateRefundCustomerNotification(order, shopAccount, payment.Amount);
                notifications.Add(noti);
                isRefund = true;
            }
            else
            {
                refundPayment.Status = PaymentStatus.PaidFail;
                isRefund = false;

                // Get moderator account to send mail
                await SendEmailAnnounceModeratorAsync(order).ConfigureAwait(false);

                // Send notification for moderator
                notifications.AddRange(NotiAnnounceRefundFail(order));
            }

            await _paymentRepository.AddAsync(refundPayment).ConfigureAwait(false);
        }

        return (notifications, isRefund);
    }

    private async Task SendEmailAnnounceModeratorAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            _emailService.SendEmailToAnnounceModeratorRefundFail(moderator.Email, order.Id);
        }
    }

    private List<Notification> NotiAnnounceRefundFail(Order order)
    {
        var notification = new List<Notification>();
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            var noti = _notificationFactory.CreateRefundFaillNotification(order, moderator);
            notification.Add(noti);
        }

        return notification;
    }
}