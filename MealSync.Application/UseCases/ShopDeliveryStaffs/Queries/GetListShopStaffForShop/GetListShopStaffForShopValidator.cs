using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopValidator : AbstractValidator<GetListShopStaffForShopQuery>
{
    public GetListShopStaffForShopValidator()
    {
        RuleFor(x => x.OrderByMode)
            .Must(x => x >= 0 && x <= 1)
            .WithMessage("Vui lòng chọn 0 hoặc 1 cho order by mode");

        RuleFor(x => x.IntendedReceiveDate)
            .Must(x => x != null || x != default)
            .WithMessage("Vui lòng cung cấp ngày nhận hàng");

        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thức đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime  + FrameConstant.TIME_FRAME_IN_MINUTES)
            .WithMessage($"Thời gian kết thúc phải lớn hơn thời gian bắt đầu {FrameConstant.TIME_FRAME_IN_MINUTES} phút");
    }
}