using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Commands.LinkFoods;

public class LinkFoodValidator : AbstractValidator<LinkFoodCommand>
{
    public LinkFoodValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id món ăn");

        RuleFor(x => x.ShopCategoryId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id doanh mục");
    }
}