using FluentValidation;

namespace MealSync.Application.UseCases.OptionGroups.Commands.DeleteOptionGroups;

public class DeleteOptionGroupValidator : AbstractValidator<DeleteOptionGroupCommand>
{
    public DeleteOptionGroupValidator()
    {
        RuleFor(og => og.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp id lựa chọn cần xóa");

        RuleFor(og => og.Id)
            .NotNull()
            .WithMessage("Vui lòng cung cấp có chấp nhận bỏ qua cảnh cáo không");
    }
}