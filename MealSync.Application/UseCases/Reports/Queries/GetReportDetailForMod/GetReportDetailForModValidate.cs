using FluentValidation;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForMod;

public class GetReportDetailForModValidate : AbstractValidator<GetReportDetailForModQuery>
{
    public GetReportDetailForModValidate()
    {
        RuleFor(q => q.ReportId)
            .GreaterThan(0)
            .WithMessage("Id phải lớn hơn 0");
    }
}