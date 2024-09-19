using FluentValidation;
using MealSync.Application.Common.Enums;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginValidate : AbstractValidator<LoginCommand>
{
    public LoginValidate()
    {
        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập");
        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("Password bắt buộc nhập");
    }
}