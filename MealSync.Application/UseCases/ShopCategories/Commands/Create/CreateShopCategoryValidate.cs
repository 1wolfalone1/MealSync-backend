using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Create;

public class CreateShopCategoryValidate : AbstractValidator<CreateShopCategoryCommand>
{
    public CreateShopCategoryValidate()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Tên thể loại bắt buộc");
    }
}