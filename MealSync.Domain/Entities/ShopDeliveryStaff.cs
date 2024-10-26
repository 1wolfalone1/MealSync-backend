using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("shop_delivery_staff")]
public class ShopDeliveryStaff : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public ShopDeliveryStaffStatus Status { get; set; }

    public virtual Account Account { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<DeliveryPackage> DeliveryPackages { get; set; } =
        new List<DeliveryPackage>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}