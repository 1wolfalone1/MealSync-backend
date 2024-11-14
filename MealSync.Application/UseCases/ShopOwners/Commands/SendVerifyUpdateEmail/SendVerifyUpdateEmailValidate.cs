using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;

public class SendVerifyUpdateEmailValidate : AbstractValidator<SendVerifyUpdateEmailCommand>
{
    public SendVerifyUpdateEmailValidate()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .When(x => !x.IsVerifyOldEmail)
            .WithMessage("Email mới không được để trống.");
    }
}