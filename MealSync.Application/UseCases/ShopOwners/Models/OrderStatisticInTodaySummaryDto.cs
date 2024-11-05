namespace MealSync.Application.UseCases.ShopOwners.Models;

public class OrderStatisticInTodaySummaryDto
{
    public string Date { get; set; }

    public int TotalOrderPending { get; set; }

    public int TotalOrderConfirmed { get; set; }

    public int TotalOrderPreparing { get; set; }

    public int TotalOrderDelivering { get; set; }

    public int TotalOrderFailDelivery { get; set; }

    public int TotalOrderCompleted { get; set; }
}