
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.AddShopOperatingSlots;

public class AddShopOperatingSlotCommand : ICommand<Result>
{
    public string Title { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}