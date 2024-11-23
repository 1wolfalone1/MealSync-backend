using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderForModerator;

public class GetOrderForModeratorValidator : AbstractValidator<GetOrderForModeratorQuery>
{
    public GetOrderForModeratorValidator()
    {
        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .Must(x => x.Value.Date <= TimeFrameUtils.GetCurrentDateInUTC7().Date)
                .WithMessage("Vui lòng cung cấp ngày bắt đầu nhỏ hơn hoặc bẳng ngày hiện tại")
                .LessThanOrEqualTo(x => x.DateTo.Value)
                .WithMessage("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

            RuleFor(x => x.DateTo)
                .Must(x => x.Value.Date <= TimeFrameUtils.GetCurrentDateInUTC7().Date)
                .WithMessage("Vui lòng cung cấp ngày kết thúc nhỏ hơn hoặc bẳng ngày hiện tại");
        });

        RuleFor(x => x.StatusMode)
            .Must(x => x >= 0 && x <= 4)
            .WithMessage("Vui lòng cung cấp status mode 0 (tất cả) | 1 (Hoàn thành) | 2 (Thực hiện) | 3 (Báo Cáo) | 4 (Đã hủy)");

        RuleFor(x => x.DormitoryMode)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Vui lòng cung cấp domitory mode 0 (tất cả)");
    }
}