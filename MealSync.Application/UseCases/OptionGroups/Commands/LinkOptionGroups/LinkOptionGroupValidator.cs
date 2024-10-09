using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Commands.LinkOptionGroups;

public class LinkOptionGroupValidator : AbstractValidator<LinkOptionGroupCommand>
{
    public LinkOptionGroupValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id món ăn");

        RuleFor(x => x.OptionGroupId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id lựa chọn phụ");
    }
}