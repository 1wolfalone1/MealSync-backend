namespace MealSync.Application.UseCases.Accounts.Models;

public class LoginResponse
{
    public AccountResponse AccountResponse { get; set; } = null!;

    public TokenResponse TokenResponse { get; set; } = null!;
}