using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.SendVerifyCode;

public class SendVerifyCodeValidate : AbstractValidator<SendVerifyCodeCommand>
{
    public SendVerifyCodeValidate()
    {
        RuleFor(a => a.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập.");
        RuleFor(a => a.VerifyType)
            .IsInEnum()
            .WithMessage("Verify type 1(Customer Register), 2(Shop Register), 3(Forgot password)");
    }
}