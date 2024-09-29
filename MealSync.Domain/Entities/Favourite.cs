using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("favourtite")]
public class Favourite : BaseEntity
{
    public long CustomerId { get; set; }

    public long ShopId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Shop Shop { get; set; }
}