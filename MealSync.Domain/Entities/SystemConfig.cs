using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("system_config")]
public class SystemConfig : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int TimeFrameDuration { get; set; }

    public int MaxFlagsBeforeBan { get; set; }

    public int MaxWarningBeforeInscreaseFlag { get; set; }
}