using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Commands.UpdateDeliveryPackageGroups;

public class UpdateDeliveryPackageGroupCommand : ICommand<Result>
{
    public List<DeliveryPackageInUpdateRequest> DeliveryPackages { get; set; }

    public bool? IsConfirm { get; set; }

    public class DeliveryPackageInUpdateRequest
    {
        public long? DeliveryPackageId { get; set; }

        public long? ShopDeliveryStaffId { get; set; }

        public long[] OrderIds { get; set; }
    }
}