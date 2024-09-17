using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Domain.Entities;

[Table("account")]
[Index("Email", Name = "account_email_unique", IsUnique = true)]
[Index("PhoneNumber", Name = "account_phone_number_unique", IsUnique = true)]
public class Account : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(200)]
    public string Email { get; set; } = null!;

    [StringLength(250)]
    public string Password { get; set; } = null!;

    [StringLength(300)]
    public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public Genders Genders { get; set; }

    public AccountTypes Type { get; set; }

    public string? FUserId { get; set; }

    public string? DeviceToken { get; set; }

    public AccountStatus Status { get; set; }

    public int NumOfFlag { get; set; }

    public long RoleId { get; set; }

    public virtual Role Role { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ShopOwner? ShopOwner { get; set; }

    public virtual Moderator? Moderator { get; set; }

    public virtual StaffDelivery? StaffDelivery { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<AccountPermission> AccountPermissions { get; set; } = new List<AccountPermission>();

    public virtual ICollection<AccountFlag> AccountFlags { get; set; } = new List<AccountFlag>();

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}