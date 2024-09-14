using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("topping_question")]
public class ProductQuestion : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    public QuestionTypes Type { get; set; }

    public string? Description { get; set; }

    public QuestionStatus Status { get; set; }

    public virtual Product Product { get; set; }

    public virtual ICollection<ProductQuestionOption> ProductQuestionOptions { get; set; } = new List<ProductQuestionOption>();
}