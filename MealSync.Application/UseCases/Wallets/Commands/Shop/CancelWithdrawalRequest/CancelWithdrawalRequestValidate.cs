using FluentValidation;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.CancelWithdrawalRequest;

public class CancelWithdrawalRequestValidate : AbstractValidator<CancelWithdrawalRequestCommand>
{
    public CancelWithdrawalRequestValidate()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}