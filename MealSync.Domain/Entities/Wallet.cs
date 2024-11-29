using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("wallet")]
public class Wallet : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public double AvailableAmount { get; set; }

    public double IncomingAmount { get; set; }

    public double ReportingAmount { get; set; }

    public DateTimeOffset NextTransferDate { get; set; }

    public WalletTypes Type { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<WalletTransaction> WalletTransactionFroms { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WalletTransaction> WalletTransactionTos { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
}