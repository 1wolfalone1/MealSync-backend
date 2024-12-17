namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopOrderStatisticDto
{
    public int Month { get; set; }

    public OrderStatisticDetailDto OrderStatisticDetail { get; set; }

    public class OrderStatisticDetailDto
    {
        public int Total { get; set; }

        public int TotalSuccess { get; set; }

        public int TotalOrderInProcess { get; set; }

        public int TotalFailOrRefund { get; set; }

        public int TotalCancelOrReject { get; set; }
    }
}