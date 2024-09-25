using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("topping_option")]
public class ProductQuestionOption : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ToppingQuestionId { get; set; }

    [Column(TypeName = "text")]
    public string Description { get; set; }

    public bool IsPricing { get; set; }

    public string? ImageUrl { get; set; }

    public double Price { get; set; }

    public ToppingOptionStatus Status { get; set; }

    public virtual ProductQuestion ProductQuestion { get; set; }

    public virtual ICollection<OrderDetailOption> OrderDetailOptions { get; set; } = new List<OrderDetailOption>();
}