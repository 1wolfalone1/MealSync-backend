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

namespace MealSync.Application.UseCases.Orders.Commands.CancelOrderCustomer;

public class CancelOrderCustomerHandler : ICommandHandler<CancelOrderCustomerCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<CancelOrderCustomerHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEmailService _emailService;

    public CancelOrderCustomerHandler(
        IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService,
        IVnPayPaymentService paymentService, IPaymentRepository paymentRepository, IUnitOfWork unitOfWork,
        ISystemResourceRepository systemResourceRepository, ILogger<CancelOrderCustomerHandler> logger,
        IAccountRepository accountRepository, INotificationFactory notificationFactory, INotifierService notifierService,
        IBuildingRepository buildingRepository, IEmailService emailService)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _logger = logger;
        _accountRepository = accountRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _buildingRepository = buildingRepository;
        _emailService = emailService;
    }

    public async Task<Result<Result>> Handle(CancelOrderCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = await _orderRepository.GetByIdAndCustomerIdIncludePayment(request.Id, customerId).ConfigureAwait(false);
        if (order == default)
        {
            // Throw exception order not found
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            var payment = order.Payments.FirstOrDefault(p => p.Type == PaymentTypes.Payment);
            if (payment == default)
            {
                // Throw exception not found payment
                throw new InvalidBusinessException(MessageCode.E_PAYMENT_NOT_FOUND.GetDescription());
            }
            else
            {
                if (order.Status == OrderStatus.Pending)
                {
                    return await CancelOrderAsync(order, payment).ConfigureAwait(false);
                }
                else if (order.Status == OrderStatus.Confirmed)
                {
                    var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
                    var intendedReceiveDateTime = new DateTime(
                        order.IntendedReceiveDate.Year,
                        order.IntendedReceiveDate.Month,
                        order.IntendedReceiveDate.Day,
                        order.StartTime / 100,
                        order.StartTime % 100,
                        0);
                    var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-TimeUtils.TIME_CANCEL_ORDER_CONFIRMED_IN_HOURS);

                    if (now < endTime)
                    {
                        return await CancelOrderAsync(order, payment).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new InvalidBusinessException(MessageCode.E_ORDER_CUSTOMER_OVERDUE_CANCEL_ORDER.GetDescription(), new object[] { TimeUtils.TIME_CANCEL_ORDER_CONFIRMED_IN_HOURS });
                    }
                }
                else
                {
                    // Throw exception can not cancel
                    throw new InvalidBusinessException(MessageCode.E_ORDER_CUSTOMER_FAIL_TO_CANCEL_ORDER.GetDescription());
                }
            }
        }
    }

    private async Task<Result<Result>> CancelOrderAsync(Order order, Payment payment)
    {
        string? refundMessage = string.Empty;
        bool isRefund;

        if (payment.PaymentMethods == PaymentMethods.VnPay && payment.Status == PaymentStatus.PaidSuccess)
        {
            // Refund + update status order to cancel
            isRefund = true;
        }
        else
        {
            // Update status order to cancel
            isRefund = false;
        }

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            if (isRefund)
            {
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
                    var options = new JsonSerializerOptions
                        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                    var content = JsonSerializer.Serialize(refundResult, options);
                    refundPayment.Status = PaymentStatus.PaidSuccess;
                    refundPayment.PaymentThirdPartyId = refundResult.VnpTransactionNo;
                    refundPayment.PaymentThirdPartyContent = content;
                    refundMessage = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_REFUND_SUCCESS.GetDescription());
                }
                else
                {
                    refundPayment.Status = PaymentStatus.PaidFail;
                    refundMessage = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_REFUND_FAIL.GetDescription());

                    // Get moderator account to send mail
                    await SendEmailAnnounceModeratorAsync(order).ConfigureAwait(false);

                    // Send notification for moderator
                    await NotifyAnnounceRefundFailAsync(order).ConfigureAwait(false);
                }

                await _paymentRepository.AddAsync(refundPayment).ConfigureAwait(false);
            }

            // Update order status to Cancelled
            order.Status = OrderStatus.Cancelled;
            order.IsRefund = isRefund;
            _orderRepository.Update(order);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                IsRefund = isRefund,
                RefundMessage = refundMessage,
                CancelMessage = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_CANCEL_SUCCESS.GetDescription()),
            });
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
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

    private async Task NotifyAnnounceRefundFailAsync(Order order)
    {
        var building = _buildingRepository.GetById(order.BuildingId);
        var moderators = _accountRepository.GetAccountsOfModeratorByDormitoryId(building!.DormitoryId);
        if (moderators != default && moderators.Count > 0)
        {
            foreach (var moderator in moderators)
            {
                var notification = _notificationFactory.CreateRefundFaillNotification(order, moderator);
                await _notifierService.NotifyAsync(notification).ConfigureAwait(false);
            }
        }
    }
}