using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("product_operating_slot")]
public class ProductOperatingSlot : BaseEntity
{
    public long OperatingSlotId { get; set; }

    public long ProductId { get; set; }

    public virtual OperatingSlot OperatingSlot { get; set; }

    public virtual Product Product { get; set; }
}