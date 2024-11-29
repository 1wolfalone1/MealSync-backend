using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("wallet_transaction")]
public class WalletTransaction : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? WalletFromId { get; set; }

    public long? WalletToId { get; set; }

    public long? WithdrawalRequestId { get; set; }

    public long? PaymentId { get; set; }

    public long? DepositId { get; set; }

    public double AvaiableAmountBefore { get; set; }

    public double IncomingAmountBefore { get; set; }

    public double ReportingAmountBefore { get; set; }

    public double Amount { get; set; }

    public WalletTransactionType Type { get; set; }

    [Column(TypeName = "text")]
    public string Description { get; set; }

    public virtual Wallet WalletFrom { get; set; }

    public virtual Wallet? WalletTo { get; set; }

    public virtual WithdrawalRequest? WithdrawalRequest { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Deposit? Deposit { get; set; }
}