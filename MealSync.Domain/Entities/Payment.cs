using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;
using Microsoft.VisualBasic;

namespace MealSync.Domain.Entities;

[Table("payment")]
public class Payment : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long OrderId { get; set; }

    public double Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public PaymentTypes Type { get; set; } = PaymentTypes.Payment;

    public PaymentMethods PaymentMethods { get; set; }

    public string? PaymentThirdPartyId { get; set; }

    public string? PaymentThirdPartyContent { get; set; }

    public virtual Order Order { get; set; }

    public virtual WalletTransaction? WalletTransaction { get; set; }
}