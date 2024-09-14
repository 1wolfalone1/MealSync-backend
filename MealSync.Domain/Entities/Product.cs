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

    public long ShopOwnerId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int TotalOrder { get; set; }

    public ProductStatus Status { get; set; }

    public bool IsSoldOut { get; set; } = false;

    public virtual ShopOwner ShopOwner { get; set; }

    public virtual ICollection<ProductOperatingHour> ProductOperatingHours { get; set; } = new List<ProductOperatingHour>();

    public virtual ICollection<ProductQuestion> ProductQuestions { get; set; } = new List<ProductQuestion>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}