using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("order_detail_product_variant")]
public class OrderDetailProductVariant : BaseEntity
{
    public long OrderDetailId { get; set; }

    public long PVariantOptionId { get; set; }

    public string PVariantName { get; set; }

    public string PVariantOptionName { get; set; }

    public string? PVariantOptionImageUrl { get; set; }

    public double Price { get; set; }

    public virtual OrderDetail OrderDetail { get; set; }

    public virtual ProductVariantOption ProductVariantOption { get; set; }
}