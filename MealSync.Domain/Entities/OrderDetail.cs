using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("order_detail")]
public class OrderDetail : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long FoodId { get; set; }

    public long OrderId { get; set; }

    public int Quantity { get; set; }

    public double BasicPrice { get; set; }

    public double TotalPrice { get; set; }

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    public string? Note { get; set; }

    public virtual Food Food { get; set; }

    public virtual Order Order { get; set; }

    public virtual ICollection<OrderDetailOption> OrderDetailOptions { get; set; } = new List<OrderDetailOption>();
}