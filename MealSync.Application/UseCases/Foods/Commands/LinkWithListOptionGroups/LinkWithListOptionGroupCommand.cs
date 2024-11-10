using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Commands.LinkWithListOptionGroups;

public class LinkWithListOptionGroupCommand : ICommand<Result>
{
    public long FoodId { get; set; }

    public long[] OptionGroupIds { get; set; }
}