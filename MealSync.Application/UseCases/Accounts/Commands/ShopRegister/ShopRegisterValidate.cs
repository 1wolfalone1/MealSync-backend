using FluentValidation;
using MealSync.Application.Common.Constants;

namespace MealSync.Application.UseCases.Accounts.Commands.ShopRegister;

public class ShopRegisterValidate : AbstractValidator<ShopRegisterCommand>
{
    public ShopRegisterValidate()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không thể trống")
            .EmailAddress()
            .WithMessage("Vui lòng cung cấp đúng email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Mật khẩu không thể để trống")
            .Matches(RegularPatternConstant.PASSWORD_PATTERN)
            .WithMessage("Mật khẩu phải từ 8 ký tự chứa 1 từ viết hoa, 1 ký tự đặc biệt");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Tên không thể để trống");

        RuleFor(x => x.ShopName)
            .NotEmpty()
            .WithMessage("Tên cửa hàng không thể để trống");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Vui lòng chọn 1 (Nam) | 2 (Nữ) | 3 (Khác)");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Địa chỉ không thể để trống");

        RuleFor(x => x.Latitude)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp đúng tọa độ");

        RuleFor(x => x.Longitude)
            .GreaterThan(0)
            .WithMessage("Vui lòng cung cấp đúng tọa độ");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Số điện thoại không thể để trống")
            .Matches(RegularPatternConstant.VN_PHONE_NUMBER_PATTERN)
            .WithMessage("Vui lòng cung cấp đúng số điện thoại");

        RuleFor(x => x.DormitoryIds)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã id của tòa ký túc");
    }
}