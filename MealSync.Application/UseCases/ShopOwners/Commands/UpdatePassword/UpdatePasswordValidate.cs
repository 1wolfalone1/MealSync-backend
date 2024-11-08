using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdatePassword;

public class UpdatePasswordValidate : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordValidate()
    {
        RuleFor(a => a.OldPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu cũ bắt buộc nhập.");

        RuleFor(a => a.NewPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu mới bắt buộc nhập.");
    }
}