using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("promotion")]
public class Promotion : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? CustomerId { get; set; }

    public long? ShopId { get; set; }

    public string Title { get; set; }

    public string? Decription { get; set; }

    public string? BannerUrl { get; set; }

    public PromotionTypes Type { get; set; }

    public double? AmountRate { get; set; }

    public double? AmountValue { get; set; }

    public double MinOrdervalue { get; set; }

    public double? MaxApplyValue { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public int UsageLimit { get; set; }

    public int NumberOfUsed { get; set; }

    public PromotionApplyTypes ApplyType { get; set; }

    public PromotionStatus Status { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}