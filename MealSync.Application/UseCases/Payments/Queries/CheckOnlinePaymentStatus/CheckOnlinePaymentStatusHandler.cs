using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Payments.Queries.CheckOnlinePaymentStatus;

public class CheckOnlinePaymentStatusHandler : IQueryHandler<CheckOnlinePaymentStatusQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public CheckOnlinePaymentStatusHandler(
        IOrderRepository orderRepository, IPaymentRepository paymentRepository,
        ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(CheckOnlinePaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var existedOrder = await _orderRepository.CheckExistedByIdAndCustomerId(request.Id, customerId).ConfigureAwait(false);
        if (existedOrder)
        {
            var payment = await _paymentRepository.GetPaymentVnPayByOrderId(request.Id).ConfigureAwait(false);
            if (payment == default)
            {
                throw new InvalidBusinessException(MessageCode.E_PAYMENT_NOT_FOUND.GetDescription());
            }
            else
            {
                var message = string.Empty;
                if (payment.Status == PaymentStatus.Pending)
                {
                    message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_PENDING.GetDescription());
                }
                else if (payment.Status == PaymentStatus.PaidCancel)
                {
                    message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_CANCEL.GetDescription());
                }
                else if (payment.Status == PaymentStatus.PaidFail)
                {
                    message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_FAIL.GetDescription());
                }
                else
                {
                    message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_SUCCESS.GetDescription());
                }

                return Result.Success(new
                {
                    OrderId = request.Id,
                    PaymentMethod = payment.PaymentMethods,
                    PaymentStatus = payment.Status,
                    Message = message,
                });
            }
        }
        else
        {
            // Throw exception order not found
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
    }
}