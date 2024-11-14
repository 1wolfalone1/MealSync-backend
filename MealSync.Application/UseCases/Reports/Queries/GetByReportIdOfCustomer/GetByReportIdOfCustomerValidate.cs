using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Queries.GetByReportIdOfCustomer;

public class GetByReportIdOfCustomerValidate : AbstractValidator<GetByReportIdOfCustomerQuery>
{
    public GetByReportIdOfCustomerValidate()
    {
        RuleFor(q => q.CustomerReportId)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}