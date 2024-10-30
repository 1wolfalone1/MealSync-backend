using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Commands.AutoAssignDeliveryPackages;

public class AutoAssignDeliveryPackageCommand : ICommand<Result>
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsShopOwnerShip { get; set; }
}