namespace MealSync.Application.UseCases.Orders.Models;

public class DeliveryInforResponse
{
    public long Id { get; set; }

    public DateTimeOffset LastestDeliveryFailAt { get; set; }

    public DateTimeOffset ReceiveAt { get; set; }

    public int DeliveryStatus { get; set; }

    public EvidenceOrderResponse DeliveryFaileEvidence { get; set; }
}