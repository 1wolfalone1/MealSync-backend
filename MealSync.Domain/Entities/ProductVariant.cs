using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("product_variant")]
public class ProductVariant : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    [Column(TypeName = "text")]
    public string? Name { get; set; }

    public ProductVariantStatus Status { get; set; }

    public virtual Product Product { get; set; }

    public virtual ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = new List<ProductVariantOption>();
}