namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountResponse
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public long RoleId { get; set; }

    public string RoleName { get; set; } = null!;
}