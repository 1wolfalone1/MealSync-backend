using System.Text.Json.Serialization;
using MealSync.Application.Common.Enums;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.Orders.Models;

public class EvidenceOrderResponse
{
    public long Id { get; set; }

    public string Reason { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string ReasonIdentity { get; set; }

    public int ReasonIndentity
    {
        get
        {
            if (!string.IsNullOrEmpty(ReasonIdentity) && ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                return 2;
            else if (!string.IsNullOrEmpty(ReasonIdentity) && ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription())
                return 1;

            return 0;
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public string EvidenceDeliveryFailJson { get; set; }

    public List<ShopDeliveyFailEvidence> Evidences
    {
        get
        {
            if (EvidenceDeliveryFailJson != null)
            {
                return JsonConvert.DeserializeObject<List<ShopDeliveyFailEvidence>>(EvidenceDeliveryFailJson);
            }

            return new List<ShopDeliveyFailEvidence>();
        }
    }
}