using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageDetailByTimeFrames;

public class GetDeliveryPackageDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}