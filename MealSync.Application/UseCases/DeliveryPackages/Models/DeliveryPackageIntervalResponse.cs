using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Models;

public class DeliveryPackageIntervalResponse
{
    public DateTime IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public List<DeliveryPackageGroupDetailResponse> DeliveryPackageGroups { get; set; } = new();

    public List<OrderDetailForShopResponse> UnassignOrders { get; set; } = new();
}