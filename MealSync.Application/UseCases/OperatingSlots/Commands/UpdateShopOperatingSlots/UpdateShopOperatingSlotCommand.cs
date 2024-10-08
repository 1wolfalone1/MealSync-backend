using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.UpdateShopOperatingSlots;

public class UpdateShopOperatingSlotCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Title { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsConfirm { get; set; }

}