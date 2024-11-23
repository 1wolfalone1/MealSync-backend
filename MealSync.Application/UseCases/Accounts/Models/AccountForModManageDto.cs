using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountForModManageDto
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public CustomerStatus Status { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}