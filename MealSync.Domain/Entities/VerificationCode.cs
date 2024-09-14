using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("verification_code")]
public class VerificationCode : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long AccountId { get; set; }

    [StringLength(100)]
    public string Code { get; set; } = null!;

    public DateTimeOffset ExpiredTịme { get; set; }

    public VerificationCodeTypes Type { get; set; }

    public VerificationCodeStatus Status { get; set; }

    public virtual Account Account { get; set; } = null!;
}