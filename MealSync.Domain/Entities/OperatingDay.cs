using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("operating_day")]
public class OperatingDay : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopOwnerId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public int AbleTotalOrder { get; set; }

    public bool IsClose { get; set; }

    public virtual ICollection<OperatingFrame> OperatingFrames { get; set; } = new List<OperatingFrame>();

    public virtual ICollection<ProductOperatingHour> ProductOperatingHours { get; set; } = new List<ProductOperatingHour>();
}