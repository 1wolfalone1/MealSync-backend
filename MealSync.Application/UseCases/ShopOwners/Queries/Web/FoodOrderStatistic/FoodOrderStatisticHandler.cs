using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Queries.Web.FoodOrderStatistic;

public class FoodOrderStatisticHandler : IQueryHandler<FoodOrderStatisticQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public FoodOrderStatisticHandler(IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(FoodOrderStatisticQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        return Result.Success(await _shopRepository.GetFoodOrderStatistic(shopId, request.DateFrom, request.DateTo).ConfigureAwait(false));
    }
}