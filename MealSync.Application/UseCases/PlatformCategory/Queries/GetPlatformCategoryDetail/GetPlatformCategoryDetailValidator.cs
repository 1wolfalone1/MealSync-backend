using FluentValidation;

namespace MealSync.Application.UseCases.PlatformCategory.Queries.GetPlatformCategoryDetail;

public class GetPlatformCategoryDetailValidator : AbstractValidator<GetPlatformCategoryDetailQuery>
{
    public GetPlatformCategoryDetailValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id");
    }
}