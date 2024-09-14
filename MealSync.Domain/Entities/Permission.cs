using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("permission")]
public class Permission : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Name { get; set; }

    public PermissionTypes Type { get; set; }

    public virtual ICollection<AccountPermission> AccountPermissions { get; set; } = new List<AccountPermission>();
}