using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Payments.Commands.RePaymentOrder;

public class RePaymentOrderHandler : ICommandHandler<RePaymentOrderCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentHistoryRepository _paymentHistoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVnPayPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RePaymentOrderHandler> _logger;

    public RePaymentOrderHandler(IPaymentRepository paymentRepository, IVnPayPaymentService paymentService,
        IUnitOfWork unitOfWork, ILogger<RePaymentOrderHandler> logger, IPaymentHistoryRepository paymentHistoryRepository)
    {
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _paymentHistoryRepository = paymentHistoryRepository;
    }

    public async Task<Result<Result>> Handle(RePaymentOrderCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetForRepaymentByOrderId(request.OrderId).ConfigureAwait(false);

        if (payment != default)
        {
            var order = _orderRepository.GetById(request.OrderId)!;
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            DateTime intendedReceiveEndDateTime;
            if (order.EndTime == 2400)
            {
                intendedReceiveEndDateTime = new DateTime(
                        order.IntendedReceiveDate.Year,
                        order.IntendedReceiveDate.Month,
                        order.IntendedReceiveDate.Day,
                        0,
                        0,
                        0)
                    .AddDays(1);
            }
            else
            {
                intendedReceiveEndDateTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    order.EndTime / 100,
                    order.EndTime % 100,
                    0);
            }
            var endTimeDateTimeOffset = new DateTimeOffset(intendedReceiveEndDateTime, TimeSpan.FromHours(7));

            if (now >= endTimeDateTimeOffset)
            {
                throw new InvalidBusinessException(MessageCode.E_ORDER_DELIVERY_END_TIME_EXCEEDED.GetDescription());
            }

            var paymentHistory = new PaymentHistory
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = payment.Status,
                Type = payment.Type,
                PaymentMethods = payment.PaymentMethods,
                PaymentThirdPartyId = payment.PaymentThirdPartyId,
                PaymentThirdPartyContent = payment.PaymentThirdPartyContent,
            };

            var newPayment = new Payment
            {
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = PaymentStatus.Pending,
                Type = PaymentTypes.Payment,
                PaymentMethods = PaymentMethods.VnPay,
                PaymentThirdPartyId = null,
                PaymentThirdPartyContent = null,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _paymentRepository.Remove(payment);
                await _paymentRepository.AddAsync(newPayment).ConfigureAwait(false);
                await _paymentHistoryRepository.AddAsync(paymentHistory).ConfigureAwait(false);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Success(new
                {
                    PaymentUrl = await _paymentService.CreatePaymentUrl(newPayment).ConfigureAwait(false),
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_PAYMENT_NOT_IN_STATUS_REPAYMENT.GetDescription(), new object[] { request.OrderId });
        }
    }
}