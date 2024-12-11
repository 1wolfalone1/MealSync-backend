using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Moderators.Queries.GetActivityLog;

public class GetActivityLogValidator : AbstractValidator<GetActivityLogQuery>
{
    public GetActivityLogValidator()
    {
        RuleFor(x => x.TargetType)
            .Must(x => x >= 0 && x <= 5);

        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .Must(x => x.Value <= TimeFrameUtils.GetCurrentDate().DateTime)
                .WithMessage("Vui lòng cung cấp ngày bắt đầu nhỏ hơn hoặc bẳng ngày hiện tại")
                .LessThanOrEqualTo(x => x.DateTo.Value)
                .WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

            RuleFor(x => x.DateTo)
                .Must(x => x.Value.Date <= TimeFrameUtils.GetCurrentDate().DateTime)
                .WithMessage("Vui lòng cung cấp ngày kết thúc nhỏ hơn hoặc bẳng ngày hiện tại");
        });
    }
}