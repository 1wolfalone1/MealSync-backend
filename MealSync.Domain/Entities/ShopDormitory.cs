using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("shop_dormitory")]
public class ShopDormitory : BaseEntity
{
    public long ShopId { get; set; }

    public long DormitoryId { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual Dormitory Dormitory { get; set; }
}