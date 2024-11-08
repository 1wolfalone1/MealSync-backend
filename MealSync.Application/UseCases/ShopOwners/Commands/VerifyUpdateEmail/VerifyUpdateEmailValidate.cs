using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.VerifyUpdateEmail;

public class VerifyUpdateEmailValidate : AbstractValidator<VerifyUpdateEmailCommand>
{
    public VerifyUpdateEmailValidate()
    {
        RuleFor(a => a.NewEmail)
            .NotEmpty()
            .WithMessage("Email mới bắt buộc nhập.");

        RuleFor(v => v.CodeVerifyNewEmail)
            .InclusiveBetween(1000, 9999)
            .WithMessage("Mã xác thực bao gồm 4 số.");
    }
}