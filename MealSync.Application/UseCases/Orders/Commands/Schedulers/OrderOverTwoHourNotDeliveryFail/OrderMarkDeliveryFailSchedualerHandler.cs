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
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.OrderOverTwoHourNotDeliveryFail;

public class OrderMarkDeliveryFailSchedualerHandler : ICommandHandler<OrderMarkDeliveryFailSchedulerCommand, BatchHistory>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderMarkDeliveryFailSchedualerHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IPaymentRepository _paymentRepository;

    public OrderMarkDeliveryFailSchedualerHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<OrderMarkDeliveryFailSchedualerHandler> logger, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IDeliveryPackageRepository deliveryPackageRepository, IAccountRepository accountRepository, IBuildingRepository buildingRepository, IEmailService emailService, ISystemConfigRepository systemConfigRepository, IBatchHistoryRepository batchHistoryRepository, IVnPayPaymentService paymentService, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, IPaymentRepository paymentRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _shopRepository = shopRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _emailService = emailService;
        _systemConfigRepository = systemConfigRepository;
        _batchHistoryRepository = batchHistoryRepository;
        _paymentService = paymentService;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<BatchHistory>> Handle(OrderMarkDeliveryFailSchedulerCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var totalRecord = 0;
        var startTime = TimeFrameUtils.GetCurrentDate();
        var endTime = TimeFrameUtils.GetCurrentDate();
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var listOrderOverTimeFrame = _orderRepository.GetListOrderOnStatusDeliveringButOverTimeFrame(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON, TimeFrameUtils.GetCurrentDateInUTC7().DateTime);
            totalRecord = listOrderOverTimeFrame != null ? listOrderOverTimeFrame.Count : 0;
            var notifications = new List<Notification>();
            var ordersUpdate = new List<Order>();
            var orderGroupByShopId = listOrderOverTimeFrame.GroupBy(o => o.ShopId);
            foreach (var shopOrder in orderGroupByShopId)
            {
                var marlDeliveryFail = await MarkShopDeliveryFailAsync(shopOrder.ToList(), shopOrder.Key).ConfigureAwait(false);
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
                BatchCode = BatchCodes.BatchCheduleMarkDeliveryFail,
                Parameter = string.Empty,
                TotalRecord = totalRecord,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startTime,
                EndDateTime = endTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return batchHistory;
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<(List<Order> Orders, List<Notification> Notifications)> MarkShopDeliveryFailAsync(List<Order> orders, long shopId)
    {
        var notfications = new List<Notification>();
        var numberOrderDeliveryFail = 0;
        foreach (var order in orders)
        {
            order.Status = OrderStatus.FailDelivery;
            order.Reason = "Cửa hàng không thực hiện việc giao hàng";
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription();
            numberOrderDeliveryFail++;
            notfications.AddRange(SendNotification(order));
            var isRefund = await RefundOrderAsync(order).ConfigureAwait(false);
            order.IsRefund = isRefund;
        }

        // Mark flag to shop account
        var shopAccount = _accountRepository.GetById(shopId);
        shopAccount.NumOfFlag += numberOrderDeliveryFail;

        // Send email for shop annouce flag increase
        var systemConfig = _systemConfigRepository.GetSystemConfig();
        if (shopAccount.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
        {
            _emailService.SendEmailToAnnounceAccountGotBanned(shopAccount.Email, shopAccount.FullName);
            shopAccount.Status = AccountStatus.Banned;
        }
        else
        {
            var idOrders = string.Join(", ", orders.Select(o => string.Concat(IdPatternConstant.PREFIX_ID, o.Id)).ToList());
            _emailService.SendEmailToAnnounceApplyFlagForShop(shopAccount.Email, shopAccount.NumOfFlag, $"Bạn không thực hiện giao các đơn hàng sau: {idOrders}");
        }

        _accountRepository.Update(shopAccount);
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
            if (refundResult.VnpResponseCode == ((int)VnPayRefundResponseCode.CODE_00).ToString("D2"))
            {
                var options = new JsonSerializerOptions()
                    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                var content = JsonSerializer.Serialize(refundResult, options);
                refundPayment.Status = PaymentStatus.PaidSuccess;
                refundPayment.PaymentThirdPartyId = refundResult.VnpTransactionNo;
                refundPayment.PaymentThirdPartyContent = content;

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

                await _walletTransactionRepository.AddRangeAsync(listWalletTransaction).ConfigureAwait(false);
                _walletRepository.Update(systemTotalWallet);
                _walletRepository.Update(systemCommissionWallet);
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

    private List<Notification> SendNotification(Order order)
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
}