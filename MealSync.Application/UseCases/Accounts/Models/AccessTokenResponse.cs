namespace MealSync.Application.UseCases.Accounts.Models;

public class AccessTokenResponse
{
    public string AccessToken { get;  set; }

    public string RefreshToken { get;  set; }
}