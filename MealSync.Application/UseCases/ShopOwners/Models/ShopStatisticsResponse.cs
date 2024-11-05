namespace MealSync.Application.UseCases.ShopOwners.Models;

public class ShopStatisticsResponse
{
    public string StartDate { get; set; }

    public string EndDate { get; set; }

    public int TotalOrderDone { get; set; }

    public int TotalOrderInProcess { get; set; }

    public double SuccessfulOrderPercentage { get; set; }

    public double Revenue { get; set; }

    public int TotalSuccess { get; set; }

    public int TotalFailOrRefund { get; set; }

    public int TotalCancelOrReject { get; set; }

    public List<TopFoodOrderDto> Foods { get; set; }
}