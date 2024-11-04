namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopStatisticSummaryResponse
{
    public OrderStatisticInTodaySummaryDto OrderStatisticInToday { get; set; }

    public OrderStatisticInMonthResponse OrderStatisticInMonth { get; set; }

    public class OrderStatisticInMonthResponse
    {
        public int Month { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public double Revenue { get; set; }

        public int TotalSuccess { get; set; }

        public int TotalFailOrRefund { get; set; }

        public int TotalCancelOrReject { get; set; }
    }
}