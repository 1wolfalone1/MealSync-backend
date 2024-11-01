using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Create;

public class CreateDeliveryStaffValidate : AbstractValidator<CreateDeliveryStaffCommand>
{
    public CreateDeliveryStaffValidate()
    {
        RuleFor(a => a.FullName)
            .NotEmpty()
            .WithMessage("Tên nhân viên bắt buộc nhập.");
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