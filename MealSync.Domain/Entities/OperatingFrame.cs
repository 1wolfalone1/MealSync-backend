using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("operating_frame")]
public class OperatingFrame : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long OperatingDayId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public bool IsActive { get; set; }

    public virtual OperatingDay OperatingDay { get; set; }
}