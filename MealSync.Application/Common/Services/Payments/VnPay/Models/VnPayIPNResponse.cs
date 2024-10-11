using System.Text.Json.Serialization;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public class VnPayIPNResponse
{
    [JsonPropertyName("RspCode")]
    public string RspCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }
}