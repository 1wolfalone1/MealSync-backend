using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Commands.LinkOptionGroups;

public class LinkOptionGroupCommand : ICommand<Result>
{
    public long OptionGroupId { get; set; }

    public long FoodId { get; set; }
}