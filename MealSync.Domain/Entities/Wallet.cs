using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public virtual ShopOwner ShopOwner { get; set; }

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}