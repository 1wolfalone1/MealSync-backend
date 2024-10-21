using System.Text.Json.Serialization;
using MealSync.Application.Common.Utils;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderDetailForShopResponse
{
    public long Id { get; set; }

    public int Status { get; set; }

    public long BuildingId { get; set; }

    public string BuildingName { get; set; }

    public double TotalPromotion { get; set; }

    public double TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? ReceiveAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public int StartTime { get; set; }

    private int _endTime; // Backing field

    public int EndTime
    {
        get
        {
            return TimeFrameUtils.ConvertEndTime(_endTime); // Use the backing field
        }

        set
        {
            _endTime = value; // Set the backing field
        }
    }

    public CustomerInforInShoprderDetailForShop Customer { get; set; }

    public PromotionInShopOrderDetail Promotion { get; set; }

    public DeliveryPackageInShopOrderDetail DeliveryPackage { get; set; }

    public List<FoodInShopOrderDetail> OrderDetails { get; set; } = new();

    public class CustomerInforInShoprderDetailForShop
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string AvartarUrl { get; set; }

        public long LocationId { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class PromotionInShopOrderDetail
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public string? BannerUrl { get; set; }

        public double AmountRate { get; set; }

        public double AmountValue { get; set; }

        public double MinOrderValue { get; set; }

        public int ApplyType { get; set; }

        public double MaximumApplyValue { get; set; }
    }

    public class DeliveryPackageInShopOrderDetail
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }
    }

    public class FoodInShopOrderDetail
    {
        public long Id { get; set; }

        public long FoodId { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice { get; set; }

        public double BasicPrice { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string OrderDescription { get; set; }

        public List<OrderDetailDescriptionDto> OptionGroups
        {
            get
            {
                if (!string.IsNullOrEmpty(OrderDescription))
                {
                    return JsonConvert.DeserializeObject<List<OrderDetailDescriptionDto>>(OrderDescription);
                }

                return new List<OrderDetailDescriptionDto>();
            }
        }
    }
}