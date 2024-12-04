using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("deposit")]
public class Deposit : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long WalletId { get; set; }

    public double Amount { get; set; }

    public DepositStatus Status { get; set; }

    public string? Description { get; set; }

    public string? PaymentThirdPartyId { get; set; }

    public string? PaymentThirdPartyContent { get; set; }

    public virtual Wallet Wallet { get; set; }

    public virtual List<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}