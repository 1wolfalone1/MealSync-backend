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

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.AutoCancelEndTimeFrame;

public class AutoCancelEndTimeFrameHandler : ICommandHandler<AutoCancelEndTimeFrameCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutoCancelEndTimeFrameCommand> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly IAccountFlagRepository _accountFlagRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;

    public AutoCancelEndTimeFrameHandler(IUnitOfWork unitOfWork, ILogger<AutoCancelEndTimeFrameCommand> logger, IOrderRepository orderRepository, INotifierService notifierService, INotificationFactory notificationFactory,
        IPaymentRepository paymentRepository, IEmailService emailService, ISystemConfigRepository systemConfigRepository, IBatchHistoryRepository batchHistoryRepository, IAccountFlagRepository accountFlagRepository,
        IBuildingRepository buildingRepository, IWalletTransactionRepository walletTransactionRepository, IAccountRepository accountRepository, IVnPayPaymentService vnPayPaymentService, IWalletRepository walletRepository, IShopRepository shopRepository, IDeliveryPackageRepository deliveryPackageRepository)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _orderRepository = orderRepository;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _paymentRepository = paymentRepository;
        _emailService = emailService;
        _systemConfigRepository = systemConfigRepository;
        _batchHistoryRepository = batchHistoryRepository;
        _accountFlagRepository = accountFlagRepository;
        _buildingRepository = buildingRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _accountRepository = accountRepository;
        _paymentService = vnPayPaymentService;
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
    }

    public async Task<Result<Result>> Handle(AutoCancelEndTimeFrameCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var listOrderOverTimeFrame = await _orderRepository.GetOrderOverEndFrameAsync(TimeFrameUtils.GetCurrentDateInUTC7().DateTime).ConfigureAwait(false);
            var deliveryPackageOverTimeFrame = _deliveryPackageRepository.GetListDeliveryPackageOverFrame(TimeFrameUtils.GetCurrentDateInUTC7().DateTime);
            totalRecord = listOrderOverTimeFrame != null ? listOrderOverTimeFrame.Count : 0;
            var notifications = new List<Notification>();
            var ordersUpdate = new List<Order>();
            var orderGroupByShopId = listOrderOverTimeFrame.GroupBy(o => o.ShopId);
            foreach (var shopOrder in orderGroupByShopId)
            {
                var marlDeliveryFail = await CheckingOrderAndHandle(shopOrder.ToList(), shopOrder.Key).ConfigureAwait(false);
                ordersUpdate.AddRange(marlDeliveryFail.Orders);
                notifications.AddRange(marlDeliveryFail.Notifications);
            }
            deliveryPackageOverTimeFrame.ForEach(dp => dp.Status = DeliveryPackageStatus.Done);
            _deliveryPackageRepository.UpdateRange(deliveryPackageOverTimeFrame);
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
                BatchCode = BatchCodes.BatchCheduleCancelOrderOverTimeFrame,
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

    private async Task<(List<Order> Orders, List<Notification> Notifications)> CheckingOrderAndHandle(List<Order> orders, long shopId)
    {
        var orderPendingAndConfirmAndPreparing = new List<Order>();
        var orderDelivering = new List<Order>();
        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Pending)
            {
                orderPendingAndConfirmAndPreparing.Add(order);
            }else if (order.Status == OrderStatus.Confirmed)
            {
                orderPendingAndConfirmAndPreparing.Add(order);
            }else if (order.Status == OrderStatus.Preparing)
            {
                orderPendingAndConfirmAndPreparing.Add(order);
            }
            else if (order.Status == OrderStatus.Delivering)
            {
                orderDelivering.Add(order);
            }
        }

        // Handle cancel
        var orderPCP = await MarkShopCancelAsync(orderPendingAndConfirmAndPreparing, shopId).ConfigureAwait(false);

        // Handle delivery fail
        var orderDeliveryFail = await MarkShopDeliveryFailAsync(orderDelivering, shopId).ConfigureAwait(false);

        var orderUpdates = orderPCP.Orders;
        orderUpdates.AddRange(orderDeliveryFail.Orders);
        var notificationUpdates = orderPCP.Notifications;
        notificationUpdates.AddRange(orderDeliveryFail.Notifications);

        return (orderUpdates, notificationUpdates);
    }

    private async Task<(List<Order> Orders, List<Notification> Notifications)> MarkShopCancelAsync(List<Order> orders, long shopId)
    {
        var notfications = new List<Notification>();
        var numberConfirmOrderOverAHour = 0;
        foreach (var order in orders)
        {
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription();
            order.Status = OrderStatus.Cancelled;
            order.Reason = "Cửa hàng không tiến hành thực hiện việc làm hoặc nhận đơn hàng";
            order.CancelAt = TimeFrameUtils.GetCurrentDate();
            numberConfirmOrderOverAHour++;
            notfications.AddRange(SendNotificationCancel(order));
            var isRefund = await RefundOrderAsync(order).ConfigureAwait(false);
            order.IsRefund = isRefund;
        }

        // Mark warning to shop account
        // Check order is in 1 hours to warning. if > 3 need to send mail warning, 5 -> flag account.
        var shop = _shopRepository.GetById(shopId);
        var shopAccount = _accountRepository.GetById(shopId);
        if (numberConfirmOrderOverAHour > 0)
        {
            var systemConfig = _systemConfigRepository.Get().FirstOrDefault();
            shop.NumOfWarning += numberConfirmOrderOverAHour;
            if (shop.NumOfWarning >= 3 && shop.NumOfWarning < systemConfig.MaxWarningBeforeInscreaseFlag)
            {
                // Send email for shop
                _emailService.SendEmailToAnnounceWarningForShop(shopAccount.Email, shop.NumOfWarning);
            }
            else if (shop.NumOfWarning >= systemConfig.MaxWarningBeforeInscreaseFlag)
            {
                // Apply flag for shop account and increase flag
                shopAccount.NumOfFlag += 1;

                // Send email for shop annouce flag increase
                if (shopAccount.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                {
                    _emailService.SendEmailToAnnounceAccountGotBanned(shopAccount.Email, shopAccount.FullName);
                    var orderProcessing = _orderRepository.CheckOrderOfShopInDeliveringAndPeparing(shopId);
                    if (orderProcessing.Count > 0)
                    {
                        shop.Status = ShopStatus.Banning;
                    }
                    else
                    {
                        shopAccount.Status = AccountStatus.Banned;
                        shop.Status = ShopStatus.Banned;
                        _accountRepository.Update(shopAccount);
                    }
                }
                else
                {
                    _emailService.SendEmailToAnnounceApplyFlagForShop(shopAccount.Email, shopAccount.NumOfFlag, "Cửa hàng bạn đã đủ 5 cảnh cáo từ hệ thống");
                }

                _accountRepository.Update(shopAccount);
                var accountFlag = new AccountFlag(AccountActionTypes.CancelConfirmOrder, shopId);
                await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);

                // Reset warning
                shop.NumOfWarning = 0;
            }
        }

        _accountRepository.Update(shopAccount);
        _shopRepository.Update(shop);
        return (orders, notfications);
    }

    private async Task<(List<Order> Orders, List<Notification> Notifications)> MarkShopDeliveryFailAsync(List<Order> orders, long shopId)
    {
        var notfications = new List<Notification>();
        var numberOrderDeliveryFail = 0;
        foreach (var order in orders)
        {
            order.Status = OrderStatus.FailDelivery;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription();
            order.LastestDeliveryFailAt = TimeFrameUtils.GetCurrentDate();
            numberOrderDeliveryFail++;
            notfications.AddRange(SendNotificationDeliveryFail(order));
        }

        return (orders, notfications);
    }

    private async Task<bool> RefundOrderAsync(Order order)
    {
        var payment = _paymentRepository.Get(p => p.OrderId == order.Id && p.PaymentMethods == PaymentMethods.VnPay && p.Status == PaymentStatus.PaidSuccess).FirstOrDefault();
        if (payment != default)
        {
            // Refund + update status order to cancel
            var refundPayment = new Payment
            {
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = PaymentStatus.Pending,
                Type = PaymentTypes.Refund,
                PaymentMethods = PaymentMethods.BankTransfer,
            };
            var refundResult = await _paymentService.CreateRefund(payment).ConfigureAwait(false);
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
                _notifierService.NotifyAsync(noti);
            }
            else
            {
                refundPayment.Status = PaymentStatus.PaidFail;

                // Get moderator account to send mail
                await SendEmailAnnounceModeratorAsync(order).ConfigureAwait(false);

                // Send notification for moderator
                NotiAnnounceRefundFailAsync(order);
            }

            await _paymentRepository.AddAsync(refundPayment).ConfigureAwait(false);
            return true;
        }

        return false;
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

    private async Task NotiAnnounceRefundFailAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            var noti = _notificationFactory.CreateRefundFaillNotification(order, moderator);
            await _notifierService.NotifyAsync(noti).ConfigureAwait(false);
        }
    }

    private List<Notification> SendNotificationDeliveryFail(Order order)
    {
        var notifications = new List<Notification>();
        var shop = _shopRepository.GetById(order.ShopId);
        // Send notification for customer
        var notiToCustomer = _notificationFactory.CreateOrderDeliveryFailedAutoByBatchToCustomerNotification(order, shop);
        notifications.Add(notiToCustomer);

        // Send notification for shop
        var notiToShop = _notificationFactory.CreateOrderDeliveryFailedAutoByBatchToShopNotification(order, shop);
        notifications.Add(notiToShop);

        // Send notification for moderator
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building.DormitoryId);
        foreach (var moderator in moderators)
        {
            var notiToModerator = _notificationFactory.CreateOrderDeliveryFailedToModeratorNotification(order, moderator, shop);
            notifications.Add(notiToModerator);
        }

        return notifications;
    }

    private List<Notification> SendNotificationCancel(Order order)
    {
        var notifications = new List<Notification>();
        var shop = _shopRepository.GetById(order.ShopId);
        // Send notification for customer
        var notiToCustomer = _notificationFactory.CreateOrderCancelAutoByBatchToCustomerNotification(order, shop);
        notifications.Add(notiToCustomer);

        // Send notification for shop
        var notiToShop = _notificationFactory.CreateOrderCancelAutoByBatchToShopNotification(order, shop);
        notifications.Add(notiToShop);

        return notifications;
    }
}