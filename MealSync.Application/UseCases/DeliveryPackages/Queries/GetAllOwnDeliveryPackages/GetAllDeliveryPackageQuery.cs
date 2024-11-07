using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackages;

public class GetAllDeliveryPackageQuery : IQuery<Result>
{
    public DeliveryPackageStatus[] Status { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public DateTime IntendedReceiveDate { get; set; }
}