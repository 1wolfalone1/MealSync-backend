using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Delete;

public class DeleteShopCategoryValidator : AbstractValidator<DeleteShopCategoryCommand>
{
    public DeleteShopCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id cần xóa");
    }
}