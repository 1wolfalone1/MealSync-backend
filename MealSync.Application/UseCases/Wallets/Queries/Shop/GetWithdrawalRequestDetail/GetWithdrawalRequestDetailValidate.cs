using FluentValidation;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestDetail;

public class GetWithdrawalRequestDetailValidate : AbstractValidator<GetWithdrawalRequestDetailQuery>
{
    public GetWithdrawalRequestDetailValidate()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}