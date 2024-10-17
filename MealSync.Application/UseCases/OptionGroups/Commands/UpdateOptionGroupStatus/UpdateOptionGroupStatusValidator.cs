using FluentValidation;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroupStatus;

public class UpdateOptionGroupStatusValidator : AbstractValidator<UpdateOptionGroupStatusCommand>
{
    public UpdateOptionGroupStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id của nhóm lựa chọn");

        RuleFor(og => og.Status)
            .Must(status => status == OptionGroupStatus.Active || status == OptionGroupStatus.UnActive)
            .WithMessage("Trạng thái của lựa chọn nhóm chỉ được là 'Active' hoặc 'Inactive'.");
    }
}