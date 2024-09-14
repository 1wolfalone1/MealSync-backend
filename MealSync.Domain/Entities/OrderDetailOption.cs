using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("order_detail_option")]
public class OrderDetailOption : BaseEntity
{
    public long OrderDetailId { get; set; }

    public long ToppingOptionId { get; set; }

    public double Price { get; set; }

    public virtual OrderDetail OrderDetail { get; set; }

    public virtual ProductQuestionOption ProductQuestionOption { get; set; }
}