using FluentValidation;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.UpdatePlatformCategory;

public class UpdatePlatformCategoryValidator : AbstractValidator<UpdatePlatformCategoryCommand>
{
    public UpdatePlatformCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id cần cập nhật");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên của loại món ăn hệ thống");
    }
}