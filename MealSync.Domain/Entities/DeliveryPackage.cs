using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("delivery_package")]
public class DeliveryPackage : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? ShopDeliveryStaffId { get; set; }

    public long? ShopId { get; set; }

    [Column(TypeName = "date")]
    public DateTime DeliveryDate { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public DeliveryPackageStatus Status { get; set; }

    public virtual ShopDeliveryStaff? ShopDeliveryStaff { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}