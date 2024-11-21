using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignDeliveryPackages;

public class SuggestAssignDeliveryPackageQuery : IQuery<Result>
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public long[] ShipperIds { get; set; }
}