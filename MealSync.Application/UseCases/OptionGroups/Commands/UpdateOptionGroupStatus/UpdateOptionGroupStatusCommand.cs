using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroupStatus;

public class UpdateOptionGroupStatusCommand : ICommand<Result>
{
    public long Id { get; set; }

    public OptionGroupStatus Status { get; set; }
}