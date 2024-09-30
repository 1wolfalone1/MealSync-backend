using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("option_group")]
public class OptionGroup : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    [Column(TypeName = "text")]
    public string? Title { get; set; }

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public OptionGroupStatus Status { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual ICollection<FoodOptionGroup> FoodOptionGroups { get; set; } = new List<FoodOptionGroup>();
}