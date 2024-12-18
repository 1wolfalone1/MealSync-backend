using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOrderChartAdminWeb;

public class GetOrderChartAdminWebValidate : AbstractValidator<GetOrderChartAdminWebQuery>
{
    public GetOrderChartAdminWebValidate()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ngày bắt đầu");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ngày kết thúc")
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("Ngày from <  to");
    }
}