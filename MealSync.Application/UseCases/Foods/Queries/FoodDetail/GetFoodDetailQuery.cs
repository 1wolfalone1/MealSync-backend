using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailQuery : IQuery<Result>
{
    public long ShopId { get; set; }

    public long FoodId { get; set; }
}