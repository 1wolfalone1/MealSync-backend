using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("option")]
public class Option : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long OptionGroupId { get; set; }

    public bool IsDefault { get; set; }

    [Column(TypeName = "text")]
    public string Title { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsCalculatePrice { get; set; }

    public double Price { get; set; }

    public OptionStatus Status { get; set; }

    public virtual OptionGroup OptionGroup { get; set; }

    public virtual ICollection<OrderDetailOption> OrderDetailOptions { get; set; } = new List<OrderDetailOption>();
}