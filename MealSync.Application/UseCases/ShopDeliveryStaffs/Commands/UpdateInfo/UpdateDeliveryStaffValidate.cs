using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfo;

public class UpdateDeliveryStaffValidate : AbstractValidator<UpdateDeliveryStaffCommand>
{
    public UpdateDeliveryStaffValidate()
    {
        RuleFor(a => a.Id)
            .GreaterThan(0)
            .WithMessage("Id nhân viên phải lớn hơn 0.");

        RuleFor(a => a.FullName)
            .NotEmpty()
            .WithMessage("Tên nhân viên bắt buộc nhập.");

        RuleFor(a => a.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại bắt buộc nhập.");

        RuleFor(a => a.Email)
            .NotEmpty()
            .WithMessage("Email bắt buộc nhập.");

        RuleFor(a => a.Gender)
            .IsInEnum()
            .WithMessage("Giới tính: 1(Male), 2(Female), 3(UnKnown)");

        RuleFor(a => a.Status)
            .IsInEnum()
            .WithMessage("Trạng thái của nhân viên giao hàng là 1(Online), 2(Offline), 3(UnActive).");
    }
}