namespace MealSync.Application.Common.Services;

public interface IFirebaseAuthenticateUserService
{
    Task<string> CreateUser(string email, string phoneNumber, string password, string name, string avatar);

    Task<string> CreateCustomerToken(string uid);

    Task DeleteUserAccount(string uid);
}