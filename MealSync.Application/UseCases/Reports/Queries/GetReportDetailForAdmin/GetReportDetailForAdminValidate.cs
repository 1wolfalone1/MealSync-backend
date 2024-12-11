using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForAdmin;

public class GetReportDetailForAdminValidate : AbstractValidator<GetReportDetailForAdminQuery>
{
    public GetReportDetailForAdminValidate()
    {
        RuleFor(q => q.ReportId)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}