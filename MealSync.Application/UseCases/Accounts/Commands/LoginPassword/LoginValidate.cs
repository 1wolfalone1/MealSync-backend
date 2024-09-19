using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginValidate : AbstractValidator<LoginCommand>
{
    public LoginValidate()
    {
        RuleFor(e => e.Role)
            .Must(r => r is >= 1 and <= 5)
            .WithMessage("Role trong khoảng từ 1 tới 5");
        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập");
        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("Password bắt buộc nhập");
    }
}