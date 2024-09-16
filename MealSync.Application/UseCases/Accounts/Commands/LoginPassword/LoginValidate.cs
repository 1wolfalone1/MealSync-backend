using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginValidate : AbstractValidator<LoginCommand>
{
    public LoginValidate()
    {
        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("sdfsdf");
        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("sdfsdf");
    }
}