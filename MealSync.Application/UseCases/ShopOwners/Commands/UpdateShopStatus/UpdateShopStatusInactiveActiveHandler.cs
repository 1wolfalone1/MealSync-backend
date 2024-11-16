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
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveHandler : ICommandHandler<UpdateShopStatusInactiveActiveCommand, Result>
{
    private readonly ILogger<UpdateShopStatusInactiveActiveHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IAccountFlagRepository _accountFlagRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBuildingRepository _buildingRepository;

    public UpdateShopStatusInactiveActiveHandler(ILogger<UpdateShopStatusInactiveActiveHandler> logger, IUnitOfWork unitOfWork, IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository,
        ISystemResourceRepository systemResourceRepository, INotifierService notifierService, INotificationFactory notificationFactory, IEmailService emailService, IAccountFlagRepository accountFlagRepository, ICurrentAccountService currentAccountService, IAccountRepository accountRepository, ISystemConfigRepository systemConfigRepository, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, IVnPayPaymentService paymentService, IPaymentRepository paymentRepository, IBuildingRepository buildingRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _systemResourceRepository = systemResourceRepository;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _emailService = emailService;
        _accountFlagRepository = accountFlagRepository;
        _currentAccountService = currentAccountService;
        _accountRepository = accountRepository;
        _systemConfigRepository = systemConfigRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopStatusInactiveActiveCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        // Warning
        if (!request.IsConfirm)
        {
                var listOrderProcessing = _orderRepository.Get(o => OrderConstant.LIST_ORDER_STATUS_IN_PROCESSING.Any(x => x == o.Status)).ToList();
                if (request.Status == ShopStatus.InActive && listOrderProcessing != default && listOrderProcessing.Count > 1)
                {
                    var numOfPendingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Pending).Count();
                    var numOfConfirmOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Confirmed).Count();
                    var numOfPreparingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Preparing).Count();
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_SHOP_HAVE_ORDER_TO_INACTIVE.GetDescription(),
                        Message = string.Format(_systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_HAVE_ORDER_TO_INACTIVE.GetDescription()),
                            numOfPendingOrder,
                            numOfConfirmOrder,
                            numOfPreparingOrder),
                    });
                }

                if (request.IsReceivingOrderPaused && listOrderProcessing != default && listOrderProcessing.Count > 1)
                {
                    var numOfPendingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Pending).Count();
                    var numOfConfirmOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Confirmed).Count();
                    var numOfPreparingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Preparing).Count();
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_ORDER_HAVE_ORDER_TO_PAUSED_RECEIVE.GetDescription(),
                        Message = string.Format(_systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_HAVE_ORDER_TO_PAUSED_RECEIVE.GetDescription()),
                            numOfPendingOrder,
                            numOfConfirmOrder,
                            numOfPreparingOrder),
                    });
                }
        }

        // Change to InActive
        if (request.Status == ShopStatus.InActive)
        {

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);

                // Process cancel order
                var listOrderProcessing = _orderRepository.Get(o => OrderConstant.LIST_ORDER_STATUS_IN_PROCESSING.Any(x => x == o.Status)).ToList();
                var numberConfirmOrderOverAHour = 0;
                foreach (var order in listOrderProcessing)
                {
                    if (order.Status == OrderStatus.Pending)
                    {
                        await RejectOrderAsync(order).ConfigureAwait(false);
                    }

                    if (order.Status == OrderStatus.Confirmed)
                    {
                        await CancelOrderConfirmedAsync(order).ConfigureAwait(false);

                        // Check see is shop cancel order late than 1 hour near time frame
                        var currentTime = TimeFrameUtils.GetCurrentDateInUTC7();
                        var currentTimeInMinutes = (currentTime.Hour * 60) + currentTime.Minute;
                        var startTimeInMinutes = TimeUtils.ConvertToMinutes(order.StartTime);
                        var deadlineInMinutes = startTimeInMinutes - currentTimeInMinutes;
                        if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
                        {
                            if (deadlineInMinutes < OrderConstant.TIME_SHOP_CANCEL_ORDER_CONFIRMED_IN_MINUTES)
                                numberConfirmOrderOverAHour++;
                        }
                    }
                }

                // Check order is in 1 hours to warning. if > 3 need to send mail warning, 5 -> flag account.
                if (numberConfirmOrderOverAHour > 0)
                {
                    var systemConfig = _systemConfigRepository.Get().FirstOrDefault();
                    shop.NumOfWarning += numberConfirmOrderOverAHour;
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
                            _emailService.SendEmailToAnnounceApplyFlagForShop(_currentPrincipalService.CurrentPrincipal, account.NumOfFlag, "Cửa hàng bạn đã đủ 5 cảnh cảo từ hệ thống");
                        }

                        _accountRepository.Update(account);
                        var accountFlag = new AccountFlag(AccountActionTypes.CancelConfirmOrder, _currentPrincipalService.CurrentPrincipalId.Value);
                        await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);

                        // Reset warning
                        shop.NumOfWarning = 0;
                    }
                }

                // Update shop status
                shop.Status = ShopStatus.InActive;
                _shopRepository.Update(shop);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                return Result.Success(new
                {
                    Code = MessageCode.I_SHOP_CHANGE_STATUS_TO_INAC_SUCC.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CHANGE_STATUS_TO_INAC_SUCC.GetDescription()),
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        // Case change to Active
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
            shop.Status = ShopStatus.Active;
            shop.IsReceivingOrderPaused = request.IsReceivingOrderPaused;
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            if (request.Status == ShopStatus.Active && !request.IsReceivingOrderPaused)
            {
                return Result.Success(new
                {
                    Code = MessageCode.I_SHOP_CHANGE_STATUS_TO_ACTIVE_SUCC.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CHANGE_STATUS_TO_ACTIVE_SUCC.GetDescription()),
                });
            }

            return Result.Success(new
            {
                Code = MessageCode.I_SHOP_CHANGE_PAUSED_RECEIVE_ORDER_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CHANGE_PAUSED_RECEIVE_ORDER_SUCCESS.GetDescription()),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task RejectOrderAsync(Order order)
    {
        order.Status = OrderStatus.Rejected;
        var isRefund = await RefundOrderAsync(order).ConfigureAwait(false);
        order.IsRefund = isRefund;
        _orderRepository.Update(order);

        // Notification for customer order rejected
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        var noti = _notificationFactory.CreateOrderRejectedNotification(order, shop);
        _notifierService.NotifyAsync(noti);
    }

    private async Task CancelOrderConfirmedAsync(Order order)
    {
        order.Status = OrderStatus.Cancelled;
        var isRefund = await RefundOrderAsync(order).ConfigureAwait(false);
        order.IsRefund = isRefund;
        _orderRepository.Update(order);

        // Notification for customer order cancelled
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        var noti = _notificationFactory.CreateOrderCancelNotification(order, shop);
        _notifierService.NotifyAsync(noti);
    }

    private async Task<bool> RefundOrderAsync(Order order)
    {
        var payment = _paymentRepository.Get(p => p.OrderId == order.Id && p.Status == PaymentStatus.PaidSuccess && p.Type == PaymentTypes.Payment).SingleOrDefault();
        if (payment != default && payment.PaymentMethods == PaymentMethods.VnPay)
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
                    WalletFromId = systemCommissionWallet.Id,
                    AvaiableAmountBefore = systemCommissionWallet.AvailableAmount,
                    IncomingAmountBefore = systemCommissionWallet.IncomingAmount,
                    ReportingAmountBefore = systemCommissionWallet.ReportingAmount,
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

    private async Task ValidateAsync(UpdateShopStatusInactiveActiveCommand request)
    {
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        if (shop.Status != ShopStatus.Active && shop.Status != ShopStatus.InActive && request.Status == ShopStatus.InActive)
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_ABLE_TO_IN_ACTIVE.GetDescription());

        if (shop.Status != ShopStatus.Active && shop.Status != ShopStatus.InActive && request.Status == ShopStatus.Active)
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_ABLE_TO_ACTIVE.GetDescription());

        var shopWallet = _walletRepository.GetById(shop.WalletId);
        if (shopWallet.AvailableAmount < MoneyUtils.AVAILABLE_AMOUNT_LIMIT)
            throw new InvalidBusinessException(MessageCode.E_SHOP_STATUS_OVER_ACCEPT_NEGATIVE_AVAILABLE_AMOUNT.GetDescription(), new object[] { MoneyUtils.FormatMoneyWithDots(Math.Abs(MoneyUtils.AVAILABLE_AMOUNT_LIMIT)) });
    }
}