using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("product")]
public class Product : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public long CategoryId { get; set; }

    public long? ParentId { get; set; }

    public long? ShopCategoryId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int TotalOrder { get; set; }

    public ProductStatus Status { get; set; }

    public bool IsSoldOut { get; set; }

    public bool IsTopping { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual Category Category { get; set; }

    public virtual Product? ParentProduct { get; set; }

    public virtual ShopCategory? ShopCategory { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductOperatingSlot> ProductOperatingSlots { get; set; } = new List<ProductOperatingSlot>();
}