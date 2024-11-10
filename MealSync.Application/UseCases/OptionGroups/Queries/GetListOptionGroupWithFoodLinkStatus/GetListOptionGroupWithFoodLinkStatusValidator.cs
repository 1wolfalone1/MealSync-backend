using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetListOptionGroupWithFoodLinkStatus;

public class GetListOptionGroupWithFoodLinkStatusValidator : AbstractValidator<GetListOptionGroupWithFoodLinkStatusQuery>
{
    public GetListOptionGroupWithFoodLinkStatusValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của món ăn");

        RuleFor(x => x.FilterMode)
            .Must(x => x >= 0 && x <= 2)
            .WithMessage("Vui lòng cung cấp 0 get all, 1 get list unlink, 2 get list linked");
    }
}