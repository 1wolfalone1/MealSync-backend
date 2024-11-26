using FluentValidation;

namespace MealSync.Application.UseCases.PlatformCategory.Commands.ReArrangePlatformCategory;

public class ReArrangePlatformCategoryValidator : AbstractValidator<ReArrangePlatformCategoryCommand>
{
    public ReArrangePlatformCategoryValidator()
    {
        RuleFor(x => x.Ids)
            .Must(x => x.Length > 0)
            .WithMessage("Vui lòng cung cấp đầy đủ id của loại món ăn");
    }
}