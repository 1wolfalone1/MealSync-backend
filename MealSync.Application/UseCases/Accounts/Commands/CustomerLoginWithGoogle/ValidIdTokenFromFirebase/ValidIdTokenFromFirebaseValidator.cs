using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.ValidIdTokenFromFirebase;

public class ValidIdTokenFromFirebaseValidator : AbstractValidator<ValidIdTokenFromFirebaseCommand>
{
    public ValidIdTokenFromFirebaseValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Id token không thể trống");
    }
}