using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("moderator_dormitory")]
public class ModeratorDormitory : BaseEntity
{
    public long ModeratorId { get; set; }

    public long DormitoryId { get; set; }

    public virtual Moderator Moderator { get; set; }

    public virtual Dormitory Dormitory { get; set; }
}