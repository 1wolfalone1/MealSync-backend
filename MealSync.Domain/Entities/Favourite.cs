using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("favourtite")]
public class Favourite : BaseEntity
{
    public long CustomerId { get; set; }

    public long ShopOwnerId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ShopOwner ShopOwner { get; set; }
}