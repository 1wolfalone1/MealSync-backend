using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;

public class RearrangeShopCategoryValidate : AbstractValidator<RearrangeShopCategoryCommand>
{
    public RearrangeShopCategoryValidate()
    {
        RuleFor(sc => sc.Ids)
            .NotEmpty()
            .WithMessage("Danh sách Id bắt buộc nhập");

        RuleFor(sc => sc.Ids)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Danh sách Id không được trùng lặp");
    }
}