using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;

namespace MealSync.Application.UseCases.Foods.Queries.ShopOwnerFood;

public class GetShopOwnerFoodHandler : IQueryHandler<GetShopOwnerFoodQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodRepository _foodRepository;
    private readonly IMapper _mapper;

    public GetShopOwnerFoodHandler(ICurrentPrincipalService currentPrincipalService, IFoodRepository foodRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _foodRepository = foodRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopOwnerFoodQuery request, CancellationToken cancellationToken)
    {
        var foods = await _foodRepository.GetShopOwnerFood(_currentPrincipalService.CurrentPrincipalId.Value).ConfigureAwait(false);
        var response = foods.Select(g => new ShopOwnerFoodResponse()
        {
            CategoryId = g.CategoryId,
            CategoryName = g.CategoryName,
            Foods = _mapper.Map<List<ShopOwnerFoodResponse.FoodResponse>>(g.Foods),
        }).ToList();
        return Result.Success(response);
    }
}