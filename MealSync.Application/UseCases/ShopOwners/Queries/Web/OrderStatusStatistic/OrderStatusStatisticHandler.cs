using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Queries.Web.OrderStatusStatistic;

public class OrderStatusStatisticHandler : IQueryHandler<OrderStatusStatisticQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public OrderStatusStatisticHandler(IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(OrderStatusStatisticQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        return Result.Success(await _shopRepository.GetShopOrderStatistic(shopId).ConfigureAwait(false));
    }
}