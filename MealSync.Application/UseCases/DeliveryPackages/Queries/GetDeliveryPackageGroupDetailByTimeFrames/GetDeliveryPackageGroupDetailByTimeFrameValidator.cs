using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageGroupDetailByTimeFrames;

public class GetDeliveryPackageGroupDetailByTimeFrameValidator : AbstractValidator<GetDeliveryPackageGroupDetailByTimeFrameQuery>
{
    public GetDeliveryPackageGroupDetailByTimeFrameValidator()
    {
        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime + FrameConstant.TIME_FRAME_IN_MINUTES)
            .WithMessage($"Thời gian kết thúc phải lớn hơn thời gian bắt đầu {FrameConstant.TIME_FRAME_IN_MINUTES} phút");

        RuleFor(x => x.IntendedRecieveDate)
            .Must(x => x != default)
            .WithMessage("Vui lòng cung cấp ngày giao hàng");
    }
}