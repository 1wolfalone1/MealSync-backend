namespace MealSync.Application.UseCases.Accounts.Models;

public class LoginResponse
{
    public AccountResponse AccountResponse { get; set; }

    public AccessTokenResponse AccessTokenResponse { get; set; }
}