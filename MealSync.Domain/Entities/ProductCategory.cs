using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("product_category")]
public class ProductCategory : BaseEntity
{
    public long ProductId { get; set; }

    public long CategoryId { get; set; }

    public Product Product { get; set; }

    public Category Category { get; set; }
}