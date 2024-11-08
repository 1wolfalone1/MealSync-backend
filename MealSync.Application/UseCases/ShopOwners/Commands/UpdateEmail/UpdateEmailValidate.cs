using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateEmail;

public class UpdateEmailValidate : AbstractValidator<UpdateEmailCommand>
{
    public UpdateEmailValidate()
    {
        RuleFor(a => a.NewEmail)
            .NotEmpty()
            .WithMessage("Email mới bắt buộc nhập.");

        RuleFor(v => v.CodeVerifyNewEmail)
            .InclusiveBetween(1000, 9999)
            .WithMessage("Mã xác thực bao gồm 4 số.");
    }
}