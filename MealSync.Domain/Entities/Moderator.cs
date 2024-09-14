using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("moderator")]
public class Moderator : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<ModeratorActivityLog> ModeratorActivityLogs { get; set; }= new List<ModeratorActivityLog>();

    public virtual ICollection<ModeratorDormitory> ModeratorDormitories { get; set; }= new List<ModeratorDormitory>();
}