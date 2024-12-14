using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUDetailForAdmin;

public class GetFPUDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}