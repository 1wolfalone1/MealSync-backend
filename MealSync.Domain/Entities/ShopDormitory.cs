using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("shop_dormitory")]
public class ShopDormitory : BaseEntity
{
    public long ShopOwnerId { get; set; }

    public long DormitoryId { get; set; }

    public virtual ShopOwner ShopOwner { get; set; }

    public virtual Dormitory Dormitory { get; set; }
}