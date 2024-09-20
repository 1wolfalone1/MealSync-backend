namespace MealSync.Application.UseCases.Accounts.Models;

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}