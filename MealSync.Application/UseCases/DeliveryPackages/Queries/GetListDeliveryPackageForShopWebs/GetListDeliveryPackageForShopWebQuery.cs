using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListDeliveryPackageForShopWebs;

public class GetListDeliveryPackageForShopWebQuery : PaginationRequest, IQuery<Result>
{
    public DateTime IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string? DeliveryShopStaffFullName { get; set; }

    public string? DeliveryPackageId { get; set; }
}