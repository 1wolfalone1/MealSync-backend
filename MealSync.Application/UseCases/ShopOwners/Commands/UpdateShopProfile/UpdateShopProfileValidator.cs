using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;

public class UpdateShopProfileValidator : AbstractValidator<UpdateShopProfileCommand>
{
    public UpdateShopProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp tên cửa hàng");

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
    }
}