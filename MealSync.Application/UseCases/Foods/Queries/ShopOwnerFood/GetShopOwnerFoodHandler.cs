using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace MealSync.Application.UseCases.Foods.Queries.ShopOwnerFood;

public class GetShopOwnerFoodHandler : IQueryHandler<GetShopOwnerFoodQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodRepository _foodRepository;
    private readonly IDapperService _dapperService;
    private readonly IMapper _mapper;

    public GetShopOwnerFoodHandler(ICurrentPrincipalService currentPrincipalService, IFoodRepository foodRepository, IMapper mapper, IDapperService dapperService)
    {
        _currentPrincipalService = currentPrincipalService;
        _foodRepository = foodRepository;
        _mapper = mapper;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetShopOwnerFoodQuery request, CancellationToken cancellationToken)
    {
        Dictionary<long, ShopOwnerFoodResponse> listCategoryDic = new Dictionary<long, ShopOwnerFoodResponse>();
        Func<ShopOwnerFoodResponse, ShopOwnerFoodResponse.FoodResponse, ShopOwnerFoodResponse> map = (parent, child) =>
        {
            if (!listCategoryDic.TryGetValue(parent.Id, out var category))
            {
                if (child.Id != 0)
                    parent.Foods.Add(child);
                listCategoryDic.Add(parent.Id, parent);
            }
            else
            {
                if (child.Id != 0)
                {
                    category.Foods.Add(child);
                    listCategoryDic.Remove(category.Id);
                    listCategoryDic.Add(category.Id, category);
                }
            }

            return parent;
        };

        await _dapperService.SelectAsync<ShopOwnerFoodResponse, ShopOwnerFoodResponse.FoodResponse, ShopOwnerFoodResponse>(
            QueryName.GetListCategoryWithFood,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId
            },
            "FoodId");
        return Result.Success(listCategoryDic.Values.ToList());
    }
}