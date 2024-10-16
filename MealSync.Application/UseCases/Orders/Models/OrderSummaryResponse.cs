using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderSummaryResponse
{
    public string ShopName { get; set; }

    public string? ShopLogoUrl { get; set; }

    public OrderStatus Status { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}