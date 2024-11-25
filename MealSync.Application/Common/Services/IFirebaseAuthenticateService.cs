using MealSync.Application.Common.Services.Models;

namespace MealSync.Application.Common.Services;

public interface IFirebaseAuthenticateService
{
    Task<bool> VerifyIdToken(string idToken);

    Task<FirebaseUser> GetFirebaseUserAsync(string idToken);
}