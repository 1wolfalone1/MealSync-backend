using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.ShopOwnerFood;

public class GetShopOwnerFoodQuery : IQuery<Result>
{
    public int FilterMode { get; set; }
}