using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("food")]
public class Food : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public long PlatformCategoryId { get; set; }

    public long? ShopCategoryId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int TotalOrder { get; set; }

    public FoodStatus Status { get; set; }

    public bool IsSoldOut { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual PlatformCategory PlatformCategory { get; set; }

    public virtual ShopCategory? ShopCategory { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<FoodOperatingSlot> FoodOperatingSlots { get; set; } = new List<FoodOperatingSlot>();

    public virtual ICollection<FoodOptionGroup> FoodOptionGroups { get; set; } = new List<FoodOptionGroup>();
}