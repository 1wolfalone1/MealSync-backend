using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("shop_category")]
public class ShopCategory : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopId { get; set; }

    public string Name { get; set; }

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}