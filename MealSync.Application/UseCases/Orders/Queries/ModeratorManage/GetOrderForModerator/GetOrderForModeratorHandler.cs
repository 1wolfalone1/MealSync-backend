using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderForModerator;

public class GetOrderForModeratorHandler : IQueryHandler<GetOrderForModeratorQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderForModeratorHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetOrderForModeratorQuery request, CancellationToken cancellationToken)
    {
        var orderPaging = _orderRepository.GetOrderForModerator(request.SearchValue, request.DateFrom, request.DateTo, request.Status, request.DormitoryIds, request.PageIndex, request.PageSize);
        var order = _mapper.Map<List<OrderListForModeratorResponse>>(orderPaging.Orders);
        return Result.Success(new PaginationResponse<OrderListForModeratorResponse>(order, orderPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}