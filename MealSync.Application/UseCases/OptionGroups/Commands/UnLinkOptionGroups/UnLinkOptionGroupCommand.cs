using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UnLinkOptionGroups;

public class UnLinkOptionGroupCommand : ICommand<Result>
{
    public long OptionGroupId { get; set; }

    public long FoodId { get; set; }
}