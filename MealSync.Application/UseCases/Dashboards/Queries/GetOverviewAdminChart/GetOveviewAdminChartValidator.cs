using FluentValidation;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Dashboards.Queries.GetOverviewAdminChart;

public class GetOveviewAdminChartValidator : AbstractValidator<GetOverviewAdminChartQuery>
{
    public GetOveviewAdminChartValidator()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ngày bắt đầu");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ngày kết thúc")
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("Ngày from <  to")
            .LessThanOrEqualTo(TimeFrameUtils.GetCurrentDateInUTC7().Date)
            .WithMessage("Dateto phải nhỏ hơn hoặc bằng ngày hiện tại");
    }
}