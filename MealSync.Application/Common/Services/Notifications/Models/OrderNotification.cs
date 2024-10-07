using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class OrderNotification
{
    public long Id { get; set; }

    public long PromotionId { get; set; }

    public long ShopId { get; set; }

    public long CustomerId { get; set; }

    public long? DeliveryPackageId { get; set; }

    public long ShopLocationId { get; set; }

    public long CustomerLocationId { get; set; }

    public long BuildingId { get; set; }

    public string BuildingName { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public double ChargeFee { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string Address { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTimeOffset IntendedReceiveAt { get; set; }

    public DateTimeOffset? ReceiveAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string? QrScanToDeliveried { get; set; }

    public string? DeliverySuccessImageUrl { get; set; }

    public bool IsRefund { get; set; } = false;

    public bool IsReport { get; set; } = false;

    public string? Reason { get; set; }
}