using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class DetailOrderCustomerResponse
{
    public long Id { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string BuildingName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public long OrderDate { get; set; }

    public long IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public long? ReceiveAt { get; set; }

    public long? CompletedAt { get; set; }

    public List<OrderDetailCustomerResponse> OrderDetails { get; set; }

    public List<PaymentOrderResponse> Payments { get; set; }

    public ShopInfoResponse ShopInfo { get; set; }

    public PromotionOrderResponse? Promotion { get; set; }

    public class OrderDetailCustomerResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public int Quantity { get; set; }

        public double BasicPrice { get; set; }

        public double TotalPrice { get; set; }

        public List<OrderDetailDescriptionDto> OptionGroups { get; set; }
    }

    public class PaymentOrderResponse
    {
        public long Id { get; set; }

        public double Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public PaymentTypes Type { get; set; }

        public PaymentMethods PaymentMethods { get; set; }
    }

    public class ShopInfoResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string? LogoUrl { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class PromotionOrderResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? BannerUrl { get; set; }

        public string? Description { get; set; }

        public PromotionTypes Type { get; set; }

        public double? AmountRate { get; set; }

        public double? MaximumApplyValue { get; set; }

        public double? AmountValue { get; set; }

        public double MinOrdervalue { get; set; }

        public long StartDate { get; set; }

        public long EndDate { get; set; }

        public PromotionApplyTypes ApplyType { get; set; }
    }
}