namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopStatisticDto
{
    public double SuccessfulOrderPercentage { get; set; }

    public double Revenue { get; set; }

    public int TotalCancelByCustomer { get; set; }

    public int TotalCancelByShop { get; set; }

    public int TotalReject { get; set; }

    public int TotalDeliveredCompleted { get; set; }

    public int TotalFailDeliveredByCustomerCompleted { get; set; }

    public int TotalFailDeliveredByShopCompleted { get; set; }

    public int TotalReportResolvedHaveRefund { get; set; }

    public int TotalReportResolved { get; set; }

    public List<TopFoodItemDto> TopFoodItems { get; set; }

    public class TopFoodItemDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int TotalOrders { get; set; }
    }
}