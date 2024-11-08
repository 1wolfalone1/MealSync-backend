using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.VerifyAccountForUpdate;

public class VerifyAccountForValidate : AbstractValidator<VerifyAccountForUpdateCommand>
{
    public VerifyAccountForValidate()
    {
        RuleFor(v => v.Code)
            .InclusiveBetween(1000, 9999)
            .WithMessage("Mã xác thực bao gồm 4 số.");
    }
}