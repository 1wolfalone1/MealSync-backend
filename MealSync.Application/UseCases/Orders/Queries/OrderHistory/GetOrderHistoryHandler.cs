using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.Orders.Queries.OrderHistory;

public class GetOrderHistoryHandler : IQueryHandler<GetOrderHistoryQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetOrderHistoryHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var data = await _orderRepository.GetByCustomerIdAndStatus(customerId, request.StatusList, request.PageIndex, request.PageSize)
            .ConfigureAwait(false);
        var result = new PaginationResponse<OrderSummaryResponse>(
            _mapper.Map<List<OrderSummaryResponse>>(data.Orders), data.TotalCount, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}