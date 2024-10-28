using FluentValidation;

namespace MealSync.Application.UseCases.Customers.Commands.UpdateProfile;

public class UpdateProfileValidate : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidate()
    {
        RuleFor(a => a.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại bắt buộc nhập.");

        RuleFor(x => x.Genders)
            .IsInEnum().WithMessage("Giới tính: 1(Male), 2(Female), 3(UnKnown)");
    }
}