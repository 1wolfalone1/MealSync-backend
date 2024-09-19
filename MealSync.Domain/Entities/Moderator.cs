using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("moderator")]
public class Moderator : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<ModeratorDormitory> ModeratorDormitories { get; set; }= new List<ModeratorDormitory>();
}