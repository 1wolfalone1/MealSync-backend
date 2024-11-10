using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllOwnDeliveryPackagesForWebs;

public class GetAllOwnDeliveryPackageForWebQuery : PaginationRequest, IQuery<Result>
{
    public DeliveryPackageStatus[] Status { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public string? DeliveryPackageId { get; set; }
}