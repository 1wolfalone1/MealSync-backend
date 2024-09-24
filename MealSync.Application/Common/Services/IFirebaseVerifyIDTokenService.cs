using MealSync.Application.Common.Shared;

namespace MealSync.Application.Common.Services;

public interface IFirebaseVerifyIDTokenService
{
    Task<bool> VerifyIdToken(string idToken);

    Task<FirebaseUser> GetFirebaseUser(string idToken);
}