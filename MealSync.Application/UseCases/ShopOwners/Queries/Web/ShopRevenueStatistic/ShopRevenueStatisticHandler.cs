using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Queries.Web.ShopRevenueStatistic;

public class ShopRevenueStatisticHandler : IQueryHandler<ShopRevenueStatisticQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public ShopRevenueStatisticHandler(IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(ShopRevenueStatisticQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        return Result.Success(await _shopRepository.GetShopRevenueEachMonthById(shopId).ConfigureAwait(false));
    }
}