using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("order_transaction_history")]
public class OrderTransactionHistory : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long OrderTransactionId { get; set; }

    public long OrderId { get; set; }

    public double Amount { get; set; }

    public OrderTransactionStatus Status { get; set; }

    public OrderTransactionTypes Type { get; set; } = OrderTransactionTypes.Payment;

    public OrderTransactionPaymentMethods PaymentMethods { get; set; }

    public string? PaymentThirdPartyId { get; set; }

    public string? PaymentThirdPartyContent { get; set; }
}