using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Models;
using Microsoft.Extensions.Configuration;

namespace MealSync.Infrastructure.Services;

public class FirebaseAuthenticateService : IFirebaseAuthenticateService, IBaseService
{
    private readonly IConfiguration _configuration;

    public FirebaseAuthenticateService(IConfiguration configuration)
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

    public async Task<bool> VerifyIdToken(string idToken)
    {
        CreateFirebaseAuth();
        var decodedToken = await FirebaseAuth.DefaultInstance
            .VerifyIdTokenAsync(idToken);
        return decodedToken != null;
    }

    public async Task<FirebaseUser> GetFirebaseUserAsync(string idToken)
    {
        CreateFirebaseAuth();
        var decodedToken = await FirebaseAuth.DefaultInstance
            .VerifyIdTokenAsync(idToken);

        var userId = decodedToken.Uid;
        var phoneNumber = decodedToken.Claims.ContainsKey("phone_number") ? decodedToken.Claims["phone_number"].ToString() : null;
        var email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
        var emailVerified = decodedToken.Claims.ContainsKey("email_verified") && (bool)decodedToken.Claims["email_verified"];
        var name = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() : null;
        var picture = decodedToken.Claims.ContainsKey("picture") ? decodedToken.Claims["picture"].ToString() : null;

        // Map to FirebaseUser object
        var firebaseUser = new FirebaseUser
        {
            UserId = userId,
            PhoneNumber = phoneNumber,
            Email = email,
            Name = name,
            Picture = picture,
            EmailVerified = emailVerified,
        };

        return firebaseUser;
    }
}