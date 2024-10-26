using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderForShopByStatusResponse
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

    public DateTime CreatedDate { get; set; }

    public long DormitoryId { get; set; }

    public string DormitoryName { get; set; }

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

    public int TotalPages { get; set; }

    public CustomerInforInOrderForShop Customer { get; set; }

    public List<FoodInOrderForShop> Foods { get; set; } = new();

    public class CustomerInforInOrderForShop
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class FoodInOrderForShop
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public int Quantity { get; set; }
    }
}