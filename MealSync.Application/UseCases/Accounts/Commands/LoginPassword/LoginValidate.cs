using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

public class LoginValidate : AbstractValidator<LoginCommand>
{
    public LoginValidate()
    {
        RuleFor(e => e.LoginContext)
            .IsInEnum()
            .WithMessage("Giá trị không hợp lệ");
        RuleFor(e => e.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập");
        RuleFor(e => e.Password)
            .NotEmpty()
            .WithMessage("Mật khẩu bắt buộc nhập");
    }
}