using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("moderator_permission")]
public class AccountPermission : BaseEntity
{
    public long PermissionId { get; set; }

    public long AccountId { get; set; }

    public string Endpoint { get; set; }

    public AccountPermissionMethods Method { get; set; }

    public Permission Permission { get; set; }

    public Account Account { get; set; }
}