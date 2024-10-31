using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackageGroupByTimeFrames;

public class GetAllDeliveryPackageGroupByTimeFrameQuery : IQuery<Result>
{
    public DateTime IntendedRecieveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}