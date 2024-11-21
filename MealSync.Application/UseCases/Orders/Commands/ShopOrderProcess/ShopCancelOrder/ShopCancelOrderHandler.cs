using System.Net;
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
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCancelOrder;

public class ShopCancelOrderHandler : ICommandHandler<ShopCancelOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<ShopCancelOrderHandler> _logger;
    private readonly IPaymentHistoryRepository _paymentHistoryRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountFlagRepository _accountFlagRepository;
    private readonly IEmailService _emailService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public ShopCancelOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IPaymentRepository paymentRepository, IVnPayPaymentService paymentService, ISystemResourceRepository systemResourceRepository, ILogger<ShopCancelOrderHandler> logger, IPaymentHistoryRepository paymentHistoryRepository, IShopRepository shopRepository, IAccountFlagRepository accountFlagRepository, IEmailService emailService, ICurrentAccountService currentAccountService, IAccountRepository accountRepository, INotifierService notifierService, INotificationFactory notificationFactory, IBuildingRepository buildingRepository, ISystemConfigRepository systemConfigRepository, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _systemResourceRepository = systemResourceRepository;
        _logger = logger;
        _paymentHistoryRepository = paymentHistoryRepository;
        _shopRepository = shopRepository;
        _accountFlagRepository = accountFlagRepository;
        _emailService = emailService;
        _currentAccountService = currentAccountService;
        _accountRepository = accountRepository;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _buildingRepository = buildingRepository;
        _systemConfigRepository = systemConfigRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<Result>> Handle(ShopCancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Valiate
        Validate(request);

        var order = _orderRepository.Get(o => o.Id == request.Id)
            .Include(o => o.Payments)
            .Include(o => o.Customer).Single();

        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);

        // Warning
        if (!request.IsConfirm.Value)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var intendedReceiveDateTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_SHOP_CANCEL_ORDER_CONFIRMED_IN_HOURS);
            if (now >= endTime)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_CANCEL_ORDER_LESS_THAN_A_HOUR.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_CANCEL_ORDER_LESS_THAN_A_HOUR.GetDescription(), order.Id),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            // Refund process
            var paymentOrder = order.Payments.FirstOrDefault(p => p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            bool isRefund = false;
            if (paymentOrder != default)
            {
                isRefund = await RefundOrderAsync(order, paymentOrder).ConfigureAwait(false);
            }

            // Check time to mark an warning
            await MarkWarningProcessAsync(shop, order).ConfigureAwait(false);

            order.Reason = request.Reason;
            order.Status = OrderStatus.Cancelled;
            order.IsRefund = isRefund;
            order.ReasonIdentity = _systemResourceRepository.GetByResourceCode(OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription());
            order.CancelAt = TimeFrameUtils.GetCurrentDate();
            _orderRepository.Update(order);
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            // Noti to customer
            var noti = _notificationFactory.CreateOrderCancelNotification(order, shop);
            _notifierService.NotifyAsync(noti);
            return Result.Success(new
            {
                OrderId = order.Id,
                Code = MessageCode.I_ORDER_SHOP_CACEL_ORDER_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_SHOP_CACEL_ORDER_SUCCESS.GetDescription(), order.Id),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopCancelOrderCommand request)
    {
        var order = _orderRepository.Get(o => o.Id == request.Id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.Id });
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

    private async Task MarkWarningProcessAsync(Shop shop, Order order)
    {
        // Check see is shop cancel order late than 1 hour near time frame
        var systemConfig = _systemConfigRepository.Get().FirstOrDefault();
        var now = TimeFrameUtils.GetCurrentDateInUTC7();
        var intendedReceiveDateTime = new DateTime(
            order.IntendedReceiveDate.Year,
            order.IntendedReceiveDate.Month,
            order.IntendedReceiveDate.Day,
            order.StartTime / 100,
            order.StartTime % 100,
            0);
        var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_SHOP_CANCEL_ORDER_CONFIRMED_IN_HOURS);
        if (now >= endTime)
        {
            shop.NumOfWarning++;
            if (shop.NumOfWarning >= 3 && shop.NumOfWarning < systemConfig.MaxWarningBeforeInscreaseFlag)
            {
                // Send email for shop
                _emailService.SendEmailToAnnounceWarningForShop(_currentPrincipalService.CurrentPrincipal, shop.NumOfWarning);
            }
            else if (shop.NumOfWarning >= systemConfig.MaxWarningBeforeInscreaseFlag)
            {
                // Apply flag for shop account and increase flag
                var account = _currentAccountService.GetCurrentAccount();
                account.NumOfFlag += 1;

                // Send email for shop annouce flag increase
                if (account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                {
                    _emailService.SendEmailToAnnounceAccountGotBanned(_currentPrincipalService.CurrentPrincipal, account.FullName);
                    account.Status = AccountStatus.Banned;
                    _accountRepository.Update(account);
                }
                else
                {
                    _emailService.SendEmailToAnnounceApplyFlagForShop(_currentPrincipalService.CurrentPrincipal, account.NumOfFlag, $"Cửa hàng bạn đã đủ 5 cảnh cảo từ hệ thống");
                }

                _accountRepository.Update(account);
                var accountFlag = new AccountFlag(AccountActionTypes.CancelConfirmOrder, _currentPrincipalService.CurrentPrincipalId.Value);
                await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);

                // Reset warning
                shop.NumOfWarning = 0;
            }
        }
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
}