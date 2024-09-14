using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("Wallet_history")]
public class WalletHistory : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long WalletId { get; set; }

    public double AvailableAmountBefore { get; set; }

    public double AvailableAmountAfter { get; set; }

    public double InComingAmountBefore { get; set; }

    public double InComingAmountAfter { get; set; }

    public DateTimeOffset NextTransferDate { get; set; }
}