using FluentValidation;

namespace MealSync.Application.UseCases.Promotions.Queries.GetEligibleAndIneligiblePromotions;

public class GetEligibleAndIneligiblePromotionValidate : AbstractValidator<GetEligibleAndIneligiblePromotionQuery>
{
    public GetEligibleAndIneligiblePromotionValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");

        RuleFor(q => q.TotalPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tổng tiền phải lớn hơn hoặc bằng 0");
    }
}