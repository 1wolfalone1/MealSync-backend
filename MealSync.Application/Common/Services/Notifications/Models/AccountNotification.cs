using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class AccountNotification
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public Genders Genders { get; set; }

    public AccountTypes Type { get; set; }

    public string? FUserId { get; set; }

    public string? DeviceToken { get; set; }

    public AccountStatus Status { get; set; }

    public int NumOfFlag { get; set; }

    public long RoleId { get; set; }
}