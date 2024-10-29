using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;

public class ShopCreateDeliveryPackageValidator : AbstractValidator<ShopCreateDeliveryPackageCommand>
{
    public ShopCreateDeliveryPackageValidator()
    {
        When(x => x.ShopDeliveryStaffId != null, (() =>
        {
            RuleFor(x => x.ShopDeliveryStaffId)
                .GreaterThan(0)
                .WithMessage("Vui lòng cung cấp id của nhân viên giao hàng");
        }));

        RuleFor(x => x.OrderIds)
            .Must(x => x.Length > 0)
            .WithMessage("Phải có ít nhất 1 đơn hàng để tạo gói hàng");

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