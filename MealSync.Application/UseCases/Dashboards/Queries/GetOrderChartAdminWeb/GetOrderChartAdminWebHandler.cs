using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOrderChartAdminWeb;

public class GetOrderChartAdminWebHandler : IQueryHandler<GetOrderChartAdminWebQuery, Result>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderChartAdminWebHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Result>> Handle(GetOrderChartAdminWebQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(await _orderRepository.GetOrderStatusChart(request.DateFrom, request.DateTo).ConfigureAwait(false));
    }
}