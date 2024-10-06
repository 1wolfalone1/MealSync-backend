using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}