using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("withdrawal_request")]
public class WithdrawalRequest : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long WalletId { get; set; }

    public double Amount { get; set; }

    public WithdrawalRequestStatus Status { get; set; } = WithdrawalRequestStatus.Pending;

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public string? Reason { get; set; }

    public virtual Wallet Wallet { get; set; }

    public virtual WalletTransaction? WalletTransaction { get; set; }
}