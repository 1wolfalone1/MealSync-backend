using System.Text.Json.Serialization;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
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

    public DateTime RejectAt { get; set; }

    public DateTimeOffset CancelAt { get; set; }

    public DateTimeOffset ResolveAt { get; set; }

    public DateTime LatestDeliveryFailAt { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public string Reason { get; set; }

    public string ReasonIdentity { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string EvidenceDeliveryFailJson { get; set; }

    public List<ShopDeliveyFailEvidence> Evidences
    {
        get
        {
            if (!string.IsNullOrEmpty(EvidenceDeliveryFailJson))
            {
                return JsonConvert.DeserializeObject<List<ShopDeliveyFailEvidence>>(EvidenceDeliveryFailJson);
            }

            return new List<ShopDeliveyFailEvidence>();
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string DeliverySuccessImageUrl { get; set; }

    public bool IsDeliveredByQR
    {
        get
        {
            return ReceiveAt.HasValue && string.IsNullOrEmpty(DeliverySuccessImageUrl);
        }
    }

    public string[] DeliverySuccessImageUrls
    {
        get
        {
            if (!string.IsNullOrEmpty(DeliverySuccessImageUrl))
            {
                return DeliverySuccessImageUrl.Split(",");
            }

            return new string[0];
        }
    }

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

    public string Note { get; set; }

    public long DormitoryId { get; set; }

    public string DormitoryName { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public int TotalPages { get; set; }

    public string OrderDetailSummary
    {
        get
        {
            if (OrderDetails.Count > 0)
            {
                var summary = string.Join(", ", OrderDetails
                    .GroupBy(od => od.FoodId)
                    .Select(g =>
                    {
                        var totalQuantity = g.Sum(od => od.Quantity);
                        return totalQuantity == 1 ? g.First().Name : $"{g.First().Name} x{totalQuantity}";
                    }));

                return summary;
            }

            return string.Empty;
        }
    }

    public string OrderDetailSummaryShort
    {
        get
        {
            if (OrderDetails.Count > 0)
            {
                var summaryShort = OrderDetails
                    .GroupBy(od => od.FoodId)
                    .Select(g => new
                    {
                        Name = g.First().Name,
                        Quantity = g.Sum(od => od.Quantity)
                    })
                    .ToList();

                var totalQuantity = summaryShort.Sum(item => item.Quantity);
                var firstItem = summaryShort.First();
                var otherItemCount = summaryShort.Count - 1;

                var summaryShortText = otherItemCount > 0
                    ? (firstItem.Quantity == 1 ? $"{firstItem.Name}" : $"{firstItem.Name} x{firstItem.Quantity}")
                      + $" +{totalQuantity - firstItem.Quantity} số lượng khác"
                    : (firstItem.Quantity == 1 ? $"{firstItem.Name}" : $"{firstItem.Name} x{firstItem.Quantity}");

                return summaryShortText;
            }

            return string.Empty;
        }
    }

    public double TotalWeight
    {
        get
        {
            return OrderDetails.Sum(od => od.Weight);
        }
    }

    public CustomerInforInShoprderDetailForShop Customer { get; set; }

    public PromotionInShopOrderDetail Promotion { get; set; }

    public ShopDeliveryStaffInShopOrderDetail ShopDeliveryStaff { get; set; }

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

    public class ShopDeliveryStaffInShopOrderDetail
    {
        public long DeliveryPackageId { get; set; }

        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsShopOwnerShip { get; set; }
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

        public string Note { get; set; }

        public double Weight { get; set; }

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