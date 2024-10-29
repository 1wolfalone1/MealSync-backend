using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageGroupDetailByTimeFrames;

public class GetDeliveryPackageGroupDetailByTimeFrameQuery : IQuery<Result>
{
    public DateTime IntendedRecieveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}