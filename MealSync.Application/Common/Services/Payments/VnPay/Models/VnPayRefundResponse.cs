using System.Text.Json.Serialization;

namespace MealSync.Application.Common.Services.Payments.VnPay.Models;

public class VnPayRefundResponse
{
    [JsonPropertyName("vnp_ResponseId")]
    public string VnpResponseId { get; set; }

    [JsonPropertyName("vnp_Command")]
    public string VnpCommand { get; set; }

    [JsonPropertyName("vnp_TmnCode")]
    public string VnpTmnCode { get; set; }

    [JsonPropertyName("vnp_TxnRef")]
    public string VnpTxnRef { get; set; }

    [JsonPropertyName("vnp_Amount")]
    public string VnpAmount { get; set; }

    [JsonPropertyName("vnp_OrderInfo")]
    public string VnpOrderInfo { get; set; }

    [JsonPropertyName("vnp_ResponseCode")]
    public string VnpResponseCode { get; set; }

    [JsonPropertyName("vnp_Message")]
    public string VnpMessage { get; set; }

    [JsonPropertyName("vnp_BankCode")]
    public string VnpBankCode { get; set; }

    [JsonPropertyName("vnp_PayDate")]
    public string VnpPayDate { get; set; }

    [JsonPropertyName("vnp_TransactionNo")]
    public string VnpTransactionNo { get; set; }

    [JsonPropertyName("vnp_TransactionType")]
    public string VnpTransactionType { get; set; }

    [JsonPropertyName("vnp_TransactionStatus")]
    public string VnpTransactionStatus { get; set; }

    [JsonPropertyName("vnp_SecureHash")]
    public string VnpSecureHash { get; set; }
}