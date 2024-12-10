namespace MealSync.Application.UseCases.Orders.Models;

public class OrderDetailsCalculateSuggestTimeResponse
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public long ShopId { get; set; }

    public long BuildingId { get; set; }

    public long DormitoryId { get; set; }

    public double Weight { get; set; }

    public string CustomerAddress { get; set; }

    public double CustomerLatitude { get; set; }

    public double CustomerLongitude { get; set; }
}