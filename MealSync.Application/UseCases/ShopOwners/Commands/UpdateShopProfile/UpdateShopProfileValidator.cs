using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;

public class UpdateShopProfileValidator : AbstractValidator<UpdateShopProfileCommand>
{
    public UpdateShopProfileValidator()
    {
        RuleFor(x => x.ShopName)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên cửa hàng");

        RuleFor(x => x.ShopOnwerName)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên chủ cửa hàng");

        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .WithMessage("Vui lòng cung cấp số điện thoại cửa hàng")
            .Matches(RegularPatternConstant.VN_PHONE_NUMBER_PATTERN)
            .WithMessage("Vui lòng cung cấp số điện thoại đúng");

        RuleFor(x => x.LogoUrl)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp logo của cửa hàng");

        RuleFor(x => x.BannerUrl)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ảnh bìa của cửa hàng");

        RuleFor(x => x.DormitoryIds)
            .Must(x => x.Length > 0)
            .WithMessage("Vui lòng chọn ít nhất 1 khu vực bán hàng");

        RuleFor(x => x.Location.Address)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp địa chỉ");

        RuleFor(x => x.Location.Latitude)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp vĩ độ của vị trí");

        RuleFor(x => x.Location.Longtiude)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp kinh độ của vị trí");
    }
}