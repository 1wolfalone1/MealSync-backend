using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using MealSync.Application.Common.Services;

namespace MealSync.Infrastructure.Services;

public class FirebaseAuthenticateUserService : BaseService, IFirebaseAuthenticateUserService
{
    private readonly IConfiguration _configuration;

    public FirebaseAuthenticateUserService(IConfiguration configuration)
    {
        _configuration = configuration;
        CreateFirebaseAuth();
    }

    public void CreateFirebaseAuth()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = _configuration["PROJECT_ID"],
            });
        }
    }

    public async Task<string> CreateUser(string email, string phoneNumber, string password, string name, string avatar)
    {
        try
        {
            phoneNumber = phoneNumber != null ? "+84" + phoneNumber.Substring(1) : null;
            var args = new UserRecordArgs()
            {
                Email = email,
                EmailVerified = true,
                PhoneNumber = phoneNumber,
                Password = password,
                DisplayName = name,
                PhotoUrl = avatar,
                Disabled = false,
            };
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

            // See the UserRecord reference doc for the contents of userRecord.
            Console.WriteLine($"Successfully created new user: {userRecord.Uid}");
            return userRecord.Uid;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error created new user: " + e.Message);
            throw;
        }
    }

    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            return userRecord != null;
        }
        catch (FirebaseAuthException ex)
        {
            if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                return false;
            }

            throw;
        }
    }

    public async Task DeleteUserAccount(string uid)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
            Console.WriteLine($"Successfully deleted user with UID: {uid}");
        }
        catch (FirebaseAuthException ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<string> CreateCustomerToken(string uid)
    {
        try
        {
            var customToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
            Console.WriteLine($"Firebase Customer Token: {uid}");
            return customToken;
        }
        catch (FirebaseAuthException ex)
        {
            Console.WriteLine("Error created Custom token: " + ex.Message);
            throw;
        }
    }
}