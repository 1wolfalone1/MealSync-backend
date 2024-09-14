using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("product_operating_hours")]
public class ProductOperatingHour : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long OperatingDayId { get; set; }

    public long ProductId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public virtual OperatingDay OperatingDay { get; set; }

    public virtual Product Product { get; set; }
}
