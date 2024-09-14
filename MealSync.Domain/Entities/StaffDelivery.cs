using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("staff_delivery")]
public class StaffDelivery : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopOwnerId { get; set; }

    public StaffDeliveryStatus Status { get; set; }

    public virtual Account Account { get; set; }

    public virtual ShopOwner ShopOwner { get; set; }

    public virtual ICollection<DeliveryOrderCombination> DeliveryOrderCombinations { get; set; } =
        new List<DeliveryOrderCombination>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}