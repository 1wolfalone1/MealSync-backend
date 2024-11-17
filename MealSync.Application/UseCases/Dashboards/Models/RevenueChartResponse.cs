using Newtonsoft.Json;

namespace MealSync.Application.UseCases.Dashboards.Models;

public class RevenueChartResponse
{
    public int ThisYear { get; set; }

    public int LastYear { get; set; }

    public double TotalOfThisYear
    {
        get
        {
            if (ThisYearStr != null)
            {
                return double.Parse(ThisYearStr);
            }

            return 0;
        }
    }

    public double TotalOflastYear
    {
        get
        {
            if (LastYearStr != null)
            {
                return double.Parse(LastYearStr);
            }

            return 0;
        }
    }

    public List<RevenueChartResponseItem> TwelveMonthRevenue
    {
        get
        {
            if (TwelveMonthRevenueStr != null)
            {
                return JsonConvert.DeserializeObject<List<RevenueChartResponseItem>>(TwelveMonthRevenueStr);
            }

            return new List<RevenueChartResponseItem>();
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string TwelveMonthRevenueStr { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string ThisYearStr { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string LastYearStr { get; set; }
}

public class RevenueChartResponseItem
{
    public double ThisYear
    {
        get
        {
            if (ThisYearStr != null)
            {
                return double.Parse(ThisYearStr);
            }

            return 0;
        }
    }

    public double LastYear
    {
        get
        {
            if (LastYearStr != null)
            {
                return double.Parse(LastYearStr);
            }

            return 0;
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string ThisYearStr { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string LastYearStr { get; set; }
}