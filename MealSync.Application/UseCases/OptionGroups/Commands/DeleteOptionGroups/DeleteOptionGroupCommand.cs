using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OptionGroups.Commands.DeleteOptionGroups;

public class DeleteOptionGroupCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool? IsConfirm { get; set; }
}