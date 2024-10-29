using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;

public class ShopCreateDeliveryPackageCommand : ICommand<Result>
{
    public List<DeliveryPackageRequest> DeliveryPackages { get; set; }

    public bool? IsConfirm { get; set; }
}

public class DeliveryPackageRequest
{
    public long? ShopDeliveryStaffId { get; set; }

    public long[] OrderIds { get; set; }
}