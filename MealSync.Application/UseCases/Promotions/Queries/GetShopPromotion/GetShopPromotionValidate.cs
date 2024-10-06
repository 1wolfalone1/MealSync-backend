using FluentValidation;

namespace MealSync.Application.UseCases.Promotions.Queries.GetShopPromotion;

public class GetShopPromotionValidate : AbstractValidator<GetShopPromotionQuery>
{
    public GetShopPromotionValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}