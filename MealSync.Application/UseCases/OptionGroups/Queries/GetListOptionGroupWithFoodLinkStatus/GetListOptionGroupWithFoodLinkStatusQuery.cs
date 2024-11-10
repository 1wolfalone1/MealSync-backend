using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetListOptionGroupWithFoodLinkStatus;

public class GetListOptionGroupWithFoodLinkStatusQuery : IQuery<Result>
{
    public long FoodId { get; set; }

    public int FilterMode { get; set; }
}