using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategory;

public class GetShopCategoryValidate : AbstractValidator<GetShopCategoryQuery>
{
    public GetShopCategoryValidate()
    {
        RuleFor(q => q.Id)
            .GreaterThan(0)
            .WithMessage("Shop id phải lớn hơn 0");
    }
}