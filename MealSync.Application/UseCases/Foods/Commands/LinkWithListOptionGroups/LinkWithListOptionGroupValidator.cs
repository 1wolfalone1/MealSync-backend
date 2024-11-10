using FluentValidation;

namespace MealSync.Application.UseCases.Foods.Commands.LinkWithListOptionGroups;

public class LinkWithListOptionGroupValidator : AbstractValidator<LinkWithListOptionGroupCommand>
{
    public LinkWithListOptionGroupValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id món ăn");

        RuleForEach(x => x.OptionGroupIds).ChildRules(rule =>
        {
            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("Vui lòng cung cấp đúng id của nhóm lựa chọn");
        });
    }
}