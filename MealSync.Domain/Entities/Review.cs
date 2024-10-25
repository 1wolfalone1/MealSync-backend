using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("review")]
public class Review : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? CustomerId { get; set; }

    public long? ShopId { get; set; }

    public long OrderId { get; set; }

    public RatingRanges Rating { get; set; } = RatingRanges.FiveStar;

    [Column(TypeName = "text")]
    public string? Comment { get; set; }

    public string? ImageUrl { get; set; }

    public ReviewEntites Entity { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual Order Order { get; set; }
}