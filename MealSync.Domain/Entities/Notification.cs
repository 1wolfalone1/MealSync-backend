using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("notification")]
public class Notification : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long ReferenceId { get; set; }

    public string? ImageUrl { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    [Column(TypeName = "text")]
    public string Data { get; set; }

    public NotificationEntityTypes EntityType { get; set; }

    [NotMapped]
    public NotificationTypes Type { get; set; }

    [NotMapped]
    public bool IsSave { get; set; }

    public bool IsRead { get; set; }

    public virtual Account Account { get; set; }
}