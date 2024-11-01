using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionByFilter;

public class GetPromotionByFilterValidate : AbstractValidator<GetPromotionByFilterQuery>
{
    public GetPromotionByFilterValidate()
    {
        // Only validate ApplyType when it has a value
        When(p => p.ApplyType != default, () =>
        {
            RuleFor(p => p.ApplyType)
                .IsInEnum()
                .WithMessage("Mã giảm giá áp dụng theo 1(Phần trăm) hoặc 2(Tiền).");
        });

        // Only validate Status when it has a value
        When(p => p.Status != default, () =>
        {
            RuleFor(p => p.Status)
                .Must(p => p.Value == PromotionStatus.Active || p.Value == PromotionStatus.UnActive)
                .WithMessage("Trạng thái phải là 1(Hoạt động) hoặc 2(Không hoạt động).");
        });
    }
}