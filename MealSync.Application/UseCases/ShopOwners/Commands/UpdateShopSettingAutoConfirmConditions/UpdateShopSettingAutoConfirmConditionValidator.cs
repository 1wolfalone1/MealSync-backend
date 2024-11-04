using FluentValidation;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirmConditions;

public class UpdateShopSettingAutoConfirmConditionValidator : AbstractValidator<UpdateShopSettingAutoConfirmConditionCommand>
{
    public UpdateShopSettingAutoConfirmConditionValidator()
    {
        RuleFor(x => x.MinOrderHoursInAdvance)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian đặt hàng gần nhất đúng định dạng hhMM");

        RuleFor(x => x.MaxOrderHoursInAdvance)
            .Must(TimeUtils.IsValidOperatingSlot)
            .WithMessage("Vui lòng cung cấp thời gian đặt hàng xa nhất đúng định dạng hhMM");

        RuleFor(x => x.MaxOrderHoursInAdvance)
            .GreaterThan(x => x.MinOrderHoursInAdvance)
            .WithMessage("Max phải lớn hơn min");
    }
}