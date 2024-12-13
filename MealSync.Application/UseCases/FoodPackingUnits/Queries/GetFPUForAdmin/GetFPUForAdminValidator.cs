using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUForAdmin;

public class GetFPUForAdminValidator : AbstractValidator<GetFPUForAdminQuery>
{
    public GetFPUForAdminValidator()
    {
        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .NotEmpty()
                .WithMessage("Vui lòng cung cấp ngày bắt đầu");

            RuleFor(x => x.DateTo.Value.Date)
                .NotEmpty()
                .WithMessage("Vui lòng cung cấp ngày kết thúc")
                .GreaterThanOrEqualTo(x => x.DateFrom.Value.Date)
                .WithMessage("Ngày from <  to")
                .LessThanOrEqualTo(TimeFrameUtils.GetCurrentDate().Date)
                .WithMessage("Dateto phải nhỏ hơn hoặc bằng ngày hiện tại");
        });
    }
}