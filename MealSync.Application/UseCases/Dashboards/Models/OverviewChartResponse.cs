namespace MealSync.Application.UseCases.Dashboards.Models;

public class OverviewChartResponse
{
    public int TotalUser { get; set; }

    public double TotalUserRate { get; set; }

    public int TotalOrder { get; set; }

    public double TotalOrderRate { get; set; }

    public double TotalTradingAmount { get; set; }

    public double TotalTradingAmountRate { get; set; }

    public double TotalChargeFee { get; set; }

    public double TotalChargeFeeRate { get; set; }

    public int NumDayCompare { get; set; }

    public void CalTotalTradingRate(double previousTrading)
    {
        if (previousTrading != 0)
            this.TotalTradingAmountRate = Math.Round((this.TotalTradingAmount - previousTrading) / previousTrading * 100, 1);
        else if (previousTrading == 0 && TotalTradingAmount == 0)
            TotalTradingAmountRate = 0;
        else
            this.TotalTradingAmountRate = 100;
    }

    public void CalTotalChargeFeeRate(double previousChargeFee)
    {
        if (previousChargeFee != 0)
            this.TotalChargeFeeRate = Math.Round((this.TotalChargeFee - previousChargeFee) / previousChargeFee * 100, 1);
        else if (previousChargeFee == 0 && TotalChargeFee == 0)
            TotalChargeFeeRate = 0;
        else
            this.TotalChargeFeeRate = 100;
    }

    public void CalTotalOrderRate(double previousOrder)
    {
        if (previousOrder != 0)
            TotalOrderRate = Math.Round((TotalOrder - previousOrder) / previousOrder * 100, 1);
        else if (previousOrder == 0 && TotalOrder == 0)
            TotalOrderRate = 0;
        else
            TotalOrderRate = 100;
    }

    public void CalTotalUserRate(double previousUser)
    {
        if (previousUser != 0)
            TotalUserRate = Math.Round((TotalUser - previousUser) / previousUser * 100, 1);
        else if (previousUser == 0 && TotalUser == 0)
            TotalUserRate = 0;
        else
            TotalUserRate = 100;
    }
}