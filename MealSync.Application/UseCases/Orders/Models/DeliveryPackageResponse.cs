using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class DeliveryPackageResponse
{
    public long Id { get; set; }

    public long? ShopDeliveryStaffId { get; set; }

    public long? ShopId { get; set; }

    public DateTime DeliveryDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public DeliveryPackageStatus Status { get; set; }

    public List<OrderForShopByStatusResponse> Orders { get; set; } = new();
}