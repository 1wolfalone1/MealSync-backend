using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UnLinkOptionGroups;

public class UnLinkOptionGroupValidator : AbstractValidator<UnLinkOptionGroupCommand>
{
    public UnLinkOptionGroupValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id món ăn");

        RuleFor(x => x.OptionGroupId)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id nhóm lựa chọn");
    }
}