using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.ShopFood;

public class GetShopFoodQuery : IQuery<Result>
{
    public long Id { get; set; }

    public string? SearchValue { get; set; }

    public long? CategoryId { get; set; }
}