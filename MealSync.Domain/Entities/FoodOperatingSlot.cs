using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("food_operating_slot")]
public class FoodOperatingSlot : BaseEntity
{
    public long OperatingSlotId { get; set; }

    public long FoodId { get; set; }

    public virtual OperatingSlot OperatingSlot { get; set; }

    public virtual Food Food { get; set; }
}