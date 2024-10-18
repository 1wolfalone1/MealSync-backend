using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderSummaryResponse
{
    public long Id { get; set; }

    public string ShopName { get; set; }

    public string? ShopLogoUrl { get; set; }

    public OrderStatus Status { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public int TotalOrderDetail { get; set; }

    public long OrderDate { get; set; }

    public long IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}