using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("order_detail_option")]
public class OrderDetailOption : BaseEntity
{
    public long OrderDetailId { get; set; }

    public long OptionId { get; set; }

    public string OptionGroupTitle { get; set; }

    public string OptionTitle { get; set; }

    public string? OptionImageUrl { get; set; }

    public double Price { get; set; }

    public virtual OrderDetail OrderDetail { get; set; }

    public virtual Option Option { get; set; }
}