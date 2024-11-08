using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopBanner;

public class UpdateShopBannerValidate : AbstractValidator<UpdateShopBannerCommand>
{
    public UpdateShopBannerValidate()
    {
        RuleFor(a => a.BannerUrl)
            .NotEmpty()
            .WithMessage("Banner mới bắt buộc nhập.");
    }
}