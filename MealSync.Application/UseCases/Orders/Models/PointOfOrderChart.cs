using System.Text.Json.Serialization;

namespace MealSync.Application.UseCases.Orders.Models;

public class PointOfOrderChart
{
    public int TotalOfOrder { get; set; }

    public int Pending { get; set; }

    public int Rejected { get; set; }

    public int ProcessingOrder { get; set; }

    public int Delivered { get; set; }

    public int FailDelivered { get; set; }

    public int Canceled { get; set; }

    public int Successful { get; set; }

    public int IssueProcessing { get; set; }

    public int Resolved { get; set; }

    public decimal TotalTradingAmount { get; set; }

    public decimal TotalChargeFee { get; set; }

    [JsonIgnore]
    public DateTime? LabelDate { get; set; }

    public string Label
    {
        get
        {
            if (LabelDate != null)
                return LabelDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            return string.Empty;
        }
    }
}