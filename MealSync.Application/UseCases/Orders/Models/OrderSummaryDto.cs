using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderSummaryDto
{
    public int TotalCount { get; set; }

    public long Id { get; set; }

    public string ShopName { get; set; }

    public string? ShopLogoUrl { get; set; }

    public OrderStatus Status { get; set; }

    public string? ReasonIdentity { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public DateTime? ReceiveAt { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }
}