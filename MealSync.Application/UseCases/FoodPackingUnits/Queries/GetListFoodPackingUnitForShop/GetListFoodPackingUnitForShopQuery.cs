using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetListFoodPackingUnitForShop;

public class GetListFoodPackingUnitForShopQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchText { get; set; }
}