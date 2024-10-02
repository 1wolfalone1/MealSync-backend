using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopOperatingSlots;

public class UpdateShopOperatingSlotValidator : AbstractValidator<UpdateShopOperatingSlotCommand>
{
    public UpdateShopOperatingSlotValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của khung thời gian");

        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime + FrameConstant.TIME_FRAME_IN_MINUTES)
            .WithMessage($"Thời gian kết thúc phải lớn hơn thời gian bắt đầu {FrameConstant.TIME_FRAME_IN_MINUTES} phút");
    }
}