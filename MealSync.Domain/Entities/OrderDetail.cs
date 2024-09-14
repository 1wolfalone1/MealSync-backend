using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("order_detail")]
public class OrderDetail : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long OrderId { get; set; }

    public int Quantity { get; set; }

    public double Price { get; set; }

    public virtual Product Product { get; set; }

    public virtual Order Order { get; set; }

    public virtual ICollection<OrderDetailOption> OrderDetailOptions { get; set; } = new List<OrderDetailOption>();
}