using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.GetMaxCarryWeightOfShop;

public class GetMaxCarryWeightOfShopHandler : IQueryHandler<GetMaxCarryWeightOfShopQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetMaxCarryWeightOfShopHandler(IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetMaxCarryWeightOfShopQuery request, CancellationToken cancellationToken)
    {
        var shopMax = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        return Result.Success(new
        {
            StaffMaxCarryWeight = shopMax.MaxCarryWeight,
        });
    }
}