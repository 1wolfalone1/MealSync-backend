using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;

namespace MealSync.Application.UseCases.ShopCategories.Queries;

public class GetAllShopCategoryHandler : IQueryHandler<GetAllShopCategoryQuery, Result>
{
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetAllShopCategoryHandler(IShopCategoryRepository shopCategoryRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _shopCategoryRepository = shopCategoryRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetAllShopCategoryQuery request, CancellationToken cancellationToken)
    {
        var shopCategories = _shopCategoryRepository.GetAllByShopId(_currentPrincipalService.CurrentPrincipalId.Value);
        var response = _mapper.Map<List<ShopCategoryResponse>>(shopCategories);
        return Result.Success(response);
    }
}