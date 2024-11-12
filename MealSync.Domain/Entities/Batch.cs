using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("batch")]
public class Batch : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public BatchCodes BatchCode { get; set; }

    public string? Parameter { get; set; }

    public BatchStatus Status { get; set; }

    public DateTimeOffset ScheduledTime { get; set; }
}