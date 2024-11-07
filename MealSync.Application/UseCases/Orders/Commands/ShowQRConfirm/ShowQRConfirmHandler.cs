using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Orders.Commands.ShowQRConfirm;

public class ShowQRConfirmHandler : ICommandHandler<ShowQRConfirmCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public ShowQRConfirmHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(ShowQRConfirmCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = await _orderRepository.GetByIdAndCustomerIdIncludePayment(request.Id, customerId).ConfigureAwait(false);
        if (order == default)
        {
            // Throw exception order not found
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else if (order.Status != OrderStatus.Delivering)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_STATUS_FOR_SHOW_QR_SCAN.GetDescription());
        }
        else
        {
            return Result.Success(new
            {
                QrUrl = order.QrScanToDeliveried,
            });
        }
    }
}