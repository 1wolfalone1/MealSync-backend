using FluentValidation;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Update;

public class UpdateShopCategoryValidator : AbstractValidator<UpdateShopCategoryCommand>
{
    public UpdateShopCategoryValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id cần cập nhật");

        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Tên thể loại bắt buộc");
    }
}