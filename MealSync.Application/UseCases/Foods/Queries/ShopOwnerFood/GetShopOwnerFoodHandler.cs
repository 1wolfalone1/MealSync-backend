using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
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
        Dictionary<long, ShopOwnerFoodResponse.FoodResponse> listOperatingDic = new Dictionary<long, ShopOwnerFoodResponse.FoodResponse>();
        Func<ShopOwnerFoodResponse, ShopOwnerFoodResponse.FoodResponse, ShopOwnerFoodResponse.FoodResponse.FoodPackingUnitOfShopResponse, ShopOwnerFoodResponse.FoodResponse.OperatingSlotInFood, ShopOwnerFoodResponse> map = (parent, child1, child2, child3) =>
        {
            if (!listCategoryDic.TryGetValue(parent.Id, out var category))
            {
                if (child1.Id != 0)
                {
                    child1.FoodPackingUnit = child2;

                    if (child3.Id != 0)
                    {
                        child1.OperatingSlots.Add(child3);
                        listOperatingDic.Add(child1.Id, child1);
                    }

                    parent.Foods.Add(child1);
                }

                listCategoryDic.Add(parent.Id, parent);
            }
            else
            {
                if (child1.Id != 0)
                {
                    child1.FoodPackingUnit = child2;

                    if (listOperatingDic.TryGetValue(child1.Id, out var food))
                    {
                        child1 = food;
                        if (child3.Id != 0)
                        {
                            child1.OperatingSlots.Add(child3);
                            listOperatingDic.Remove(child1.Id);
                            listOperatingDic.Add(child1.Id, child1);
                            var foodInCategory = category.Foods.Where(f => f.Id == child1.Id).Single();
                            category.Foods.Remove(foodInCategory);
                            category.Foods.Add(child1);
                        }
                    }
                    else
                    {
                        child1.FoodPackingUnit = child2;

                        if (child3.Id != 0)
                        {
                            child1.OperatingSlots.Add(child3);
                        }

                        category.Foods.Add(child1);
                        listOperatingDic.Add(child1.Id, child1);
                    }

                    listCategoryDic.Remove(category.Id);
                    listCategoryDic.Add(category.Id, category);
                }
            }

            return parent;
        };

        await _dapperService.SelectAsync<ShopOwnerFoodResponse, ShopOwnerFoodResponse.FoodResponse, ShopOwnerFoodResponse.FoodResponse.FoodPackingUnitOfShopResponse, ShopOwnerFoodResponse.FoodResponse.OperatingSlotInFood, ShopOwnerFoodResponse>(
            QueryName.GetListCategoryWithFood,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                CurrentHours = TimeFrameUtils.GetCurrentDateInUTC7().ToString("HHmm"),
                StartLastTwoHour = TimeFrameUtils.GetCurrentDateInUTC7().AddHours(2).ToString("HHmm"),
                FilterMode = request.FilterMode,
            },
            "FoodId, FoodPackingUnitSection, OperatingSection");

        if (request.FilterMode == 0)
            return Result.Success(listCategoryDic.Values.ToList());
        else
        {
            var list = listCategoryDic.Values.ToList().Where(c => c.Foods.Count > 0);
            return Result.Success(list);
        }
    }
}