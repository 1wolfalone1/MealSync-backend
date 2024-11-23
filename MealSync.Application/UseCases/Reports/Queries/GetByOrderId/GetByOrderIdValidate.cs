using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Queries.GetByOrderId;

public class GetByOrderIdValidate : AbstractValidator<GetByOrderIdQuery>
{
    public GetByOrderIdValidate()
    {
        RuleFor(q => q.OrderId)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}