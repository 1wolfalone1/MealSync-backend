using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("food_option_group")]
public class FoodOptionGroup : BaseEntity
{
    public long FoodId { get; set; }

    public long OptionGroupId { get; set; }

    public int DisplayOrder { get; set; }

    public virtual OptionGroup OptionGroup { get; set; }

    public virtual Food Food { get; set; }
}