using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class OrderListForModeratorResponse
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public double ChargeFee { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public string TimeFrameFormat
    {
        get
        {
            return TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
        }
    }

    public DateTimeOffset OrderDate { get; set; }

    public DateTime IntendedReceiveDate { get; set; }

    public DateTimeOffset ReceiveAt { get; set; }

    public DateTimeOffset CancelAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public DateTimeOffset ResolveAt { get; set; }

    public OrderStatus Status { get; set; }

    public BuildingOrderList Buidling { get; set; }

    public DormitoryOrderList Dormitory { get; set; }

    public ShopInforOrderList Shop { get; set; }

    public class BuildingOrderList
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class DormitoryOrderList
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class ShopInforOrderList
    {
        public long Id { get; set; }

        public string ShopName { get; set; }

        public string FullName { get; set; }

        public string LogoUrl { get; set; }

        public string BannerUrl { get; set; }
    }
}