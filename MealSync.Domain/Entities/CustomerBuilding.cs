using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("customer_building")]
public class CustomerBuilding : BaseEntity
{
    public long BuildingId { get; set; }

    public long CustomerId { get; set; }

    public bool IsDefault { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Building Building { get; set; }
}