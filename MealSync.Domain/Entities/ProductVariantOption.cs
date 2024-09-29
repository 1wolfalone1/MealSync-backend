using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("product_variant_option")]
public class ProductVariantOption : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ProductVariantId { get; set; }

    public bool IsDefault { get; set; }

    [Column(TypeName = "text")]
    public string Name { get; set; }

    public string? ImageUrl { get; set; }

    public double Price { get; set; }

    public ProductVariantOptionStatus Status { get; set; }

    public virtual ProductVariant ProductVariant { get; set; }

    public virtual ICollection<OrderDetailProductVariant> OrderDetailProductVariants { get; set; } = new List<OrderDetailProductVariant>();
}