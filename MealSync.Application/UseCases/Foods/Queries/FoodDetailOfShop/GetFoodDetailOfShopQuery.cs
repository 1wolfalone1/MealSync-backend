using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetailOfShop;

public class GetFoodDetailOfShopQuery : IQuery<Result>
{
    public long Id { get; set; }
}