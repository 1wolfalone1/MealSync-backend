using FluentValidation;

namespace MealSync.Application.UseCases.Reviews.Queries.GetOverviewOfShop;

public class GetOverviewOfShopValidate : AbstractValidator<GetOverviewOfShopQuery>
{
    public GetOverviewOfShopValidate()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("ShopId phải lớn hơn 0");
    }
}