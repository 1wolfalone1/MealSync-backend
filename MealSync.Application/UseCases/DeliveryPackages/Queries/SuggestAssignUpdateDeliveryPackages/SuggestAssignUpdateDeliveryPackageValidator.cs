using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignUpdateDeliveryPackages;

public class SuggestAssignUpdateDeliveryPackageValidator : AbstractValidator<SuggestAssignUpdateDeliveryPackageQuery>
{
    public SuggestAssignUpdateDeliveryPackageValidator()
    {
        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime + FrameConstant.TIME_FRAME_IN_MINUTES)
            .WithMessage($"Thời gian kết thúc phải lớn hơn thời gian bắt đầu {FrameConstant.TIME_FRAME_IN_MINUTES} phút");

        RuleFor(x => x.EndTime)
            .GreaterThan(TimeFrameUtils.GetCurrentHoursInUTC7())
            .WithMessage("Vui lòng cung cấp khoảng thời gian hiện tại và tương lai");

        RuleFor(x => x.ShipperIds)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ít nhất 1 nhân viên");

        RuleFor(x => x.StaffMaxCarryWeight)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp số cân nặng tối đa mà nhân viên có thể mang");
    }
}