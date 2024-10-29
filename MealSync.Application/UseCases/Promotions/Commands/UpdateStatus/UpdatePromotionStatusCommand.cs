using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Commands.UpdateStatus;

public class UpdatePromotionStatusCommand : ICommand<Result>
{
    public long Id { get; set; }

    public PromotionStatus Status { get; set; }
}