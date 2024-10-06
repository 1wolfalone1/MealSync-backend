using FluentValidation;

namespace MealSync.Application.UseCases.Shops.Queries.ShopInfo;

public class GetShopInfoValidate : AbstractValidator<GetShopInfoQuery>
{
    public GetShopInfoValidate()
    {
        RuleFor(q => q.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}