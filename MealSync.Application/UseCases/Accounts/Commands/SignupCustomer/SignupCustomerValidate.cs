using FluentValidation;

namespace MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;

public class SignupCustomerValidate : AbstractValidator<SignupCustomerCommand>
{
    public SignupCustomerValidate()
    {
        RuleFor(a => a.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại bắt buộc nhập.");
        RuleFor(a => a.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập.");
        RuleFor(a => a.Password)
            .NotEmpty()
            .WithMessage("Mật khẩu bắt buộc nhập.");
    }
}