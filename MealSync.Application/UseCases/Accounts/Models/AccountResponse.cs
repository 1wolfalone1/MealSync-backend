namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountResponse
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string RoleName { get; set; }

    public string Email { get; set; }

    public string AvatarUrl { get; set; }
}