using FluentValidation;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.CreatePlatformCategory;

public class CreatePlatformCategoryValidator : AbstractValidator<CreatePlatformCategoryCommand>
{
    public CreatePlatformCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên của loại món ăn hệ thống");
    }
}