using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopOperatingSlots;

public class UpdateShopOperatingSlotValidator : AbstractValidator<UpdateShopOperatingSlotCommand>
{
    public UpdateShopOperatingSlotValidator()
    {
        RuleFor(x => x.StartTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM");

        RuleFor(x => x.EndTime)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian kết thúc đúng định dạng hhMM")
            .GreaterThanOrEqualTo(x => x.StartTime + 30)
            .WithMessage("Thời gian kết thúc phải lớn hơn thời gian bắt đầu 30 phút");
    }
}