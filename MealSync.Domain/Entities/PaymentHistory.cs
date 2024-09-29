using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("payment_history")]
public class PaymentHistory : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long PaymentId { get; set; }

    public long OrderId { get; set; }

    public double Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public PaymentTypes Type { get; set; } = PaymentTypes.Payment;

    public PaymentMethods PaymentMethods { get; set; }

    public string? PaymentThirdPartyId { get; set; }

    public string? PaymentThirdPartyContent { get; set; }
}