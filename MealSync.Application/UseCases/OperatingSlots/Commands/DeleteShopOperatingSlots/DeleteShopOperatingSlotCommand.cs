using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.DeleteShopOperatingSlots;

public class DeleteShopOperatingSlotCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }
}