using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.AddShopOperatingSlots;

public class AddShopOperatingSlotCommand : ICommand<Result>
{
    public string Title { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsReceivingOrderPaused { get; set; }
}