using FluentValidation;

namespace MealSync.Application.UseCases.Moderators.Commands.UpdateModerator;

public class UpdateModeratorValidator : AbstractValidator<UpdateModeratorCommand>
{
    public UpdateModeratorValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vui lòng cung id");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Vui lòng cung cấp đúng email");

        RuleFor(x => x.DormitoryIds)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Cần ít nhất 1 khu ký túc xá");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Vui lòng cung cấp status từ 1 - 4");
    }
}