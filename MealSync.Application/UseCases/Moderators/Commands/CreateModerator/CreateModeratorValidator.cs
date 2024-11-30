using FluentValidation;

namespace MealSync.Application.UseCases.Moderators.Commands.CreateModerator;

public class CreateModeratorValidator : AbstractValidator<CreateModeratorCommand>
{
    public CreateModeratorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Vui lòng cung cấp đúng email");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại không thể để trống")
            .Matches(RegularPatternConstant.VN_PHONE_NUMBER_PATTERN)
            .WithMessage("Vui lòng cung cấp đúng số điện thoại");

        RuleFor(x => x.DormitoryIds)
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Cần ít nhất 1 khu ký túc xá");
    }
}