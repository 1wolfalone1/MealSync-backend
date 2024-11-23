using System.Net;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopRejectOrder;

public class ShopRejectOrderHandler : ICommandHandler<ShopRejectOrderCommand, Result>
{
    private readonly INotifierService _notifierService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopRejectOrderHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IShopRepository _shopRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IEmailService _emailService;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public ShopRejectOrderHandler(INotifierService notifierService, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<ShopRejectOrderHandler> logger, ICurrentPrincipalService currentPrincipalService,
        INotificationFactory notificationFactory, IShopRepository shopRepository, ISystemResourceRepository systemResourceRepository, IEmailService emailService, IVnPayPaymentService paymentService,
        IPaymentRepository paymentRepository, IBuildingRepository buildingRepository, IAccountRepository accountRepository, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository)
    {
        _notifierService = notifierService;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _notificationFactory = notificationFactory;
        _shopRepository = shopRepository;
        _systemResourceRepository = systemResourceRepository;
        _emailService = emailService;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _buildingRepository = buildingRepository;
        _accountRepository = accountRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<Result>> Handle(ShopRejectOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.Get(o => o.Id == request.Id)
                .Include(o => o.Payments)
                .Include(o => o.Customer).Single();

            // Refund process
            var paymentOrder = order.Payments.FirstOrDefault(p => p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess);
            bool isRefund = false;
            if (paymentOrder != default)
            {
                isRefund = await RefundOrderAsync(order, paymentOrder).ConfigureAwait(false);
            }

            order.Status = OrderStatus.Rejected;
            order.Reason = request.Reason;
            order.IsRefund = isRefund;
            order.RejectAt = TimeFrameUtils.GetCurrentDate();
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Send notification to customer
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            var noti = _notificationFactory.CreateOrderRejectedNotification(order, shop);
            _notifierService.NotifyAsync(noti);
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_REJECT_SUCCESS.GetDescription(), order.Id),
                Code = MessageCode.I_ORDER_REJECT_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopRejectOrderCommand request)
    {
        var order = _orderRepository.Get(o => o.Id == request.Id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Pending)
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
}