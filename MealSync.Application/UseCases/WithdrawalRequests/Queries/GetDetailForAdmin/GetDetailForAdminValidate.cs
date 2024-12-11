using FluentValidation;
using MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForMod;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForAdmin;

public class GetDetailForAdminValidate : AbstractValidator<GetDetailForAdminQuery>
{
    public GetDetailForAdminValidate()
    {
        RuleFor(x => x.WithdrawalRequestId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}