using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.ValidIdTokenFromFirebase;

public class ValidIdTokenFromFirebaseCommand : ICommand<Result>
{
    public string IdToken { get; set; }
}