using System.Data;
using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategoryDetail;

public class GetShopCategoryDetailValidator : AbstractValidator<GetShopCategoryDetailQuery>
{
    public GetShopCategoryDetailValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");
    }
}