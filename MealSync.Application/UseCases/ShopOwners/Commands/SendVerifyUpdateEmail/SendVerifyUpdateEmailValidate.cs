using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;

public class SendVerifyUpdateEmailValidate : AbstractValidator<SendVerifyUpdateEmailCommand>
{
    public SendVerifyUpdateEmailValidate()
    {
    }
}