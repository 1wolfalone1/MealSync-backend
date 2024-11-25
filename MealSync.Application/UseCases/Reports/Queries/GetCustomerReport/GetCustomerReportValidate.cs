using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Queries.GetCustomerReport;

public class GetCustomerReportValidate : AbstractValidator<GetCustomerReportQuery>
{
    public GetCustomerReportValidate()
    {
        RuleFor(q => q.OrderId)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}