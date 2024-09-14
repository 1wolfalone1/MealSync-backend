using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("moderator_activity_log")]
public class ModeratorActivityLog : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ModeratorId { get; set; }

    public ModeratorActionTypes ActionType { get; set; }

    public ModeratorTargetTypes TargetType { get; set; }

    public long? TargetId { get; set; }

    [Column(TypeName = "text")]
    public string ActionDetail { get; set; }

    public bool IsSuccess { get; set; }

    public virtual Moderator Moderator { get; set; }
}