using System.Text.Json.Serialization;

namespace MealSync.Application.UseCases.Orders.Models;

public class DeliveryInforResponse
{
    public long Id { get; set; }

    public DateTimeOffset LastestDeliveryFailAt { get; set; }

    public DateTimeOffset ReceiveAt { get; set; }

    public bool IsDeliveredByQR { get; set; }

    [JsonIgnore]
    public string DeliverySuccessImageUrl { get; set; }

    public string[] DeliverySuccessImageUrls
    {
        get
        {
            if (!string.IsNullOrEmpty(DeliverySuccessImageUrl))
            {
                return DeliverySuccessImageUrl.Split(",");
            }

            return new string[0];
        }
    }

    public int DeliveryStatus { get; set; }

    public EvidenceOrderResponse DeliveryFaileEvidence { get; set; }
}