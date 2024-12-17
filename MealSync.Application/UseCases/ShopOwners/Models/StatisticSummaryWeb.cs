namespace MealSync.Application.UseCases.ShopOwners.Models;

public class StatisticSummaryWeb
{
    public double Revenue { get; set; }

    public double TotalPromotion { get; set; }

    public long TotalOrder { get; set; }

    public long TotalCustomer { get; set; }
}