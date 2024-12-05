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
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Shops.Commands.ModeratorManage.UpdateShopStatus;

public class UpdateShopStatusHandler : ICommandHandler<UpdateShopStatusCommand, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateShopStatusHandler> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IVnPayPaymentService _vnPayPaymentService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemConfigRepository _systemConfigRepository;

    public UpdateShopStatusHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IShopRepository shopRepository, IUnitOfWork unitOfWork, ISystemResourceRepository systemResourceRepository,
        IEmailService emailService, ILogger<UpdateShopStatusHandler> logger, IOrderRepository orderRepository,
        IWalletTransactionRepository walletTransactionRepository, IWalletRepository walletRepository,
        IPaymentRepository paymentRepository, IVnPayPaymentService vnPayPaymentService, INotificationFactory notificationFactory,
        INotifierService notifierService, IBuildingRepository buildingRepository,
        IAccountRepository accountRepository, ISystemConfigRepository systemConfigRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _shopRepository = shopRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _emailService = emailService;
        _logger = logger;
        _orderRepository = orderRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _walletRepository = walletRepository;
        _paymentRepository = paymentRepository;
        _vnPayPaymentService = vnPayPaymentService;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _buildingRepository = buildingRepository;
        _accountRepository = accountRepository;
        _systemConfigRepository = systemConfigRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopStatusCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();
        var isSendMailApprove = false;
        var isSendMailUnBan = false;
        var shop = await _shopRepository.GetShopManage(request.Id, dormitoryIds).ConfigureAwait(false);

        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            var totalOrderInProcess = await _orderRepository.CountTotalOrderInProcessByShopId(shop.Id).ConfigureAwait(false);

            if (!request.IsConfirm && shop.Status == ShopStatus.UnApprove && request.Status == ShopStatus.InActive)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_MODERATOR_UPDATE_STATUS_UN_APPROVE_TO_INACTIVE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_MODERATOR_UPDATE_STATUS_UN_APPROVE_TO_INACTIVE.GetDescription()),
                });
            }
            else if (!request.IsConfirm && (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned) && request.Status == ShopStatus.InActive)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_MODERATOR_UPDATE_STATUS_BANNED_TO_INACTIVE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_MODERATOR_UPDATE_STATUS_BANNED_TO_INACTIVE.GetDescription()),
                });
            }
            else if (!request.IsConfirm && shop.Status != ShopStatus.Banned && request.Status == ShopStatus.Banned)
            {
                if (totalOrderInProcess > 0 && shop.Status == ShopStatus.Banning)
                {
                    throw new InvalidBusinessException(MessageCode.E_MODERATOR_CAN_NOT_UPDATE_STATUS_TO_BANNED.GetDescription());
                }
                else if (totalOrderInProcess > 0 && shop.Status != ShopStatus.Banning)
                {
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_MODERATOR_UPDATE_STATUS_TO_BANNING.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_MODERATOR_UPDATE_STATUS_TO_BANNING.GetDescription()),
                    });
                }
                else
                {
                    // Ban shop
                    shop.Status = ShopStatus.Banned;
                    shop.Account.Status = AccountStatus.Banned;
                }
            }
            else if (request.IsConfirm && shop.Status == ShopStatus.UnApprove && request.Status == ShopStatus.InActive)
            {
                // Approve shop
                isSendMailApprove = true;
                shop.Status = request.Status;
            }
            else if (request.IsConfirm && (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned) && request.Status == ShopStatus.InActive)
            {
                var systemConfig = _systemConfigRepository.GetSystemConfig();

                // Ban to Active
                isSendMailUnBan = true;
                shop.Status = request.Status;
                shop.Account.Status = AccountStatus.Verify;

                if (shop.Account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                {
                    shop.Account.NumOfFlag -= 1;
                }
            }
            else if (request.IsConfirm && totalOrderInProcess > 0 && shop.Status != ShopStatus.Banned && request.Status == ShopStatus.Banned)
            {
                // Banning shop
                if (shop.Status == ShopStatus.Banning)
                {
                    throw new InvalidBusinessException(MessageCode.E_MODERATOR_CAN_NOT_UPDATE_STATUS_TO_BANNED.GetDescription());
                }
                else
                {
                    shop.Status = ShopStatus.Banning;
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }

            var ordersCancel = new List<Order>();
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _shopRepository.Update(shop);

                if (request.Status == ShopStatus.Banned)
                {
                    ordersCancel = await _orderRepository.GetForSystemCancelByShopId(shop.Id).ConfigureAwait(false);
                    await CancelOrderPendingOrConfirmed(ordersCancel).ConfigureAwait(false);
                }

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                if (request.Status == ShopStatus.Banned)
                {
                    // Notify cancel order for shop
                    foreach (var order in ordersCancel)
                    {
                        var notification = _notificationFactory.CreateOrderCancelNotification(order, shop);
                        _notifierService.NotifyAsync(notification);
                    }

                    // Send mail ban shop
                    _emailService.SendBanShopWithReason(shop.Account.Email, shop.Account.FullName, shop.Name, request.Reason, shop.Account.NumOfFlag, shop.Status == ShopStatus.Banned);
                }
                else if (isSendMailApprove)
                {
                    _emailService.SendApproveShop(shop.Account.Email, shop.Account.FullName, shop.Name);
                }
                else if (isSendMailUnBan)
                {
                    _emailService.SendUnBanShopWithReason(shop.Account.Email, shop.Account.FullName, shop.Name, request.Reason);
                }

                return Result.Success(new
                {
                    Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
    }

    private async Task CancelOrderPendingOrConfirmed(List<Order> ordersCancelBeforeBan)
    {

        foreach (var order in ordersCancelBeforeBan)
        {
            order.Status = OrderStatus.Cancelled;
            order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription();

            var payment = order.Payments.FirstOrDefault(p => p.PaymentMethods == PaymentMethods.VnPay && p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            if (payment != default)
            {
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
                order.IsRefund = true;

                if (refundResult.VnpResponseCode == ((int)VnPayRefundResponseCode.CODE_00).ToString("D2"))
                {
                    refundPayment.Status = PaymentStatus.PaidSuccess;

                    // Rút tiền từ ví hoa hồng về ví hệ thống sau đó refund tiền về cho customer
                    var systemTotalWallet = await _walletRepository.GetByType(WalletTypes.SystemTotal).ConfigureAwait(false);
                    var systemCommissionWallet = await _walletRepository.GetByType(WalletTypes.SystemCommission).ConfigureAwait(false);

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

                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemCommissionToSystemTotal).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionAddFromSystemCommissionToSystemTotal).ConfigureAwait(false);
                    await _walletTransactionRepository.AddAsync(transactionWithdrawalSystemTotalForRefundPaymentOnline).ConfigureAwait(false);
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
            }

            _orderRepository.Update(order);
        }
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