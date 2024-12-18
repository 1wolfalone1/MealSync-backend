namespace MealSync.Application.UseCases.Dashboards.Models;

public class OrderStatusChartDto
{
    public int TotalOfOrder { get; set; }

    public int TotalSuccess { get; set; }

    public int TotalOrderInProcess { get; set; }

    public int TotalFailOrRefund { get; set; }

    public int TotalCancelOrReject { get; set; }

    public DateTime LabelDate { get; set; }
}