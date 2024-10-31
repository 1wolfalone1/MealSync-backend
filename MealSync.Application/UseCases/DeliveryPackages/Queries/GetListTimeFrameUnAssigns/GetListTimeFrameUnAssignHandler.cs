using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListTimeFrameUnAssigns;

public class GetListTimeFrameUnAssignHandler : IQueryHandler<GetListTimeFrameUnAssignQuery, Result>
{
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetListTimeFrameUnAssignHandler(IDeliveryPackageRepository deliveryPackageRepository, IOrderRepository orderRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _deliveryPackageRepository = deliveryPackageRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetListTimeFrameUnAssignQuery request, CancellationToken cancellationToken)
    {
        var timeFrames = _orderRepository.GetListTimeFrameUnAssignByReceiveDate(request.IntendedReceiveDate, _currentPrincipalService.CurrentPrincipalId.Value);
        var listTimeFrame = _mapper.Map<List<TimeFrameResponse>>(timeFrames);
        var totalOrder = listTimeFrame.Sum(tf => tf.NumberOfOrder);
        return Result.Success(new
        {
            TotalOrder = totalOrder,
            timeFrames = listTimeFrame,
        });
    }
}