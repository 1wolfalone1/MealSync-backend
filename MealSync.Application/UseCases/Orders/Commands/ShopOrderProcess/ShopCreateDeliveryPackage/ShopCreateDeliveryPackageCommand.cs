using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;

public class ShopCreateDeliveryPackageCommand : ICommand<Result>
{
    public long? ShopDeliveryStaffId { get; set; }

    public long[] OrderIds { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}