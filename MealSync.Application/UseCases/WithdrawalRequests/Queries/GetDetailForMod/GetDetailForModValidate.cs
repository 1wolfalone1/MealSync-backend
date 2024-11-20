using FluentValidation;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForMod;

public class GetDetailForModValidate : AbstractValidator<GetDetailForModQuery>
{
    public GetDetailForModValidate()
    {
        RuleFor(x => x.WithdrawalRequestId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}