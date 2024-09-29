using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("staff_delivery")]
public class StaffDelivery : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public StaffDeliveryStatus Status { get; set; }

    public virtual Account Account { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<DeliveryPackage> DeliveryPackages { get; set; } =
        new List<DeliveryPackage>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}