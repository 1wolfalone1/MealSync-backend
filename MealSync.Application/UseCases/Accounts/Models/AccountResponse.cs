namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountResponse
{
    public long Id { get; set; }

    public string Email { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public string RoleName { get; set; }

    public BuildingResponse? Building { get; set; }
}