namespace MealSync.Application.UseCases.ShopOwners.Models;

public class OrderStatisticInMonthSummaryDto
{
    public double Revenue { get; set; }

    public int TotalCancelByCustomer { get; set; }

    public int TotalCancelByShop { get; set; }

    public int TotalReject { get; set; }

    public int TotalDeliveredCompleted { get; set; }

    public int TotalDeliveredResolvedRejectReport { get; set; }

    public int TotalFailDeliveredByCustomerCompleted { get; set; }

    public int TotalFailDeliveredByShopCompleted { get; set; }

    public int TotalReportResolvedHaveRefund { get; set; }

    public int TotalReportResolvedNotHaveRefund { get; set; }
}