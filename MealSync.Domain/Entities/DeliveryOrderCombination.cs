using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("delivery_order_combination")]
public class DeliveryOrderCombination : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long StaffDeliveryId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public StaffDelivery StaffDelivery { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}