using FluentValidation;
using MealSync.Application.UseCases.Accounts.Models;

namespace MealSync.Application.UseCases.Accounts.Commands.VerifyCode;

public class VerifyCodeValidate : AbstractValidator<VerifyCodeCommand>
{
    public VerifyCodeValidate()
    {
        When(v => v.VerifyType == VerifyType.ForgotPassword && !v.IsVerify, () =>
        {
            RuleFor(v => v.Password)
                .NotEmpty()
                .WithMessage("Mật khẩu bắt buộc nhập.");
        });

        RuleFor(v => v.Code)
            .InclusiveBetween(1000, 9999)
            .WithMessage("Mã xác thực bao gồm 4 số.");

        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập.");
        RuleFor(v => v.VerifyType)
            .IsInEnum()
            .WithMessage("Verify type 1(Customer Register), 2(Shop Register), 3(Forgot password)");
    }
}