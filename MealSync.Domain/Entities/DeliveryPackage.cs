using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("delivery_package")]
public class DeliveryPackage : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? StaffDeliveryId { get; set; }

    public long? ShopId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public virtual StaffDelivery? StaffDelivery { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}