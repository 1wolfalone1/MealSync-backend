using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderResponse
{
    public long Id { get; set; }

    public string BuildingName { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public DateTimeOffset? ReceiveAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public List<OrderDetailResponse> OrderDetails { get; set; }

    public class OrderDetailResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public int Quantity { get; set; }

        public double BasicPrice { get; set; }

        public double TotalPrice { get; set; }

        public List<OrderDetailDescriptionDto> OptionGroups { get; set; }
    }
}