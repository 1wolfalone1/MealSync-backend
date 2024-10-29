using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListTimeFrameUnAssigns;

public class GetListTimeFrameUnAssignHandler : IQueryHandler<GetListTimeFrameUnAssignQuery, Result>
{
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetListTimeFrameUnAssignHandler(IDeliveryPackageRepository deliveryPackageRepository, IOrderRepository orderRepository, IMapper mapper)
    {
        _deliveryPackageRepository = deliveryPackageRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetListTimeFrameUnAssignQuery request, CancellationToken cancellationToken)
    {
        var timeFrames = _orderRepository.GetListTimeFrameUnAssignByReceiveDate(request.IntendedRecieveDate);
        return Result.Success(_mapper.Map<List<TimeFrameResponse>>(timeFrames));
    }
}