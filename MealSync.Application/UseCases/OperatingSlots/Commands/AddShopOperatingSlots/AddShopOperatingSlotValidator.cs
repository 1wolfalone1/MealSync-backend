using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.AddShopOperatingSlots;

public class AddShopOperatingSlotValidator : AbstractValidator<AddShopOperatingSlotCommand>
{
    public AddShopOperatingSlotValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tiêu đề cho khung thời gian");

        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime  + FrameConstant.TIME_FRAME_IN_MINUTES)
            .WithMessage($"Thời gian kết thúc phải lớn hơn thời gian bắt đầu {FrameConstant.TIME_FRAME_IN_MINUTES} phút");

        When(x => !x.IsActive, () =>
        {
            RuleFor(x => x.IsReceivingOrderPaused)
                .Must(x => !x)
                .WithMessage("Không có trường hợp khoảng thời gian bán hàng tắt và tạm ngưng nhận hàng");
        });
    }
}