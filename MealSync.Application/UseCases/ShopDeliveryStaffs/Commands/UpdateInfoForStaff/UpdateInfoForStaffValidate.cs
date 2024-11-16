using FluentValidation;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfoForStaff;

public class UpdateInfoForStaffValidate : AbstractValidator<UpdateInfoForStaffCommand>
{
    public UpdateInfoForStaffValidate()
    {
        RuleFor(a => a.FullName)
            .NotEmpty()
            .WithMessage("Tên nhân viên bắt buộc nhập.");

        RuleFor(a => a.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại bắt buộc nhập.");

        RuleFor(a => a.Gender)
            .IsInEnum()
            .WithMessage("Giới tính: 1(Male), 2(Female), 3(UnKnown)");
    }
}