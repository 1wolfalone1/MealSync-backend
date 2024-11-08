using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopLogo;

public class UpdateShopLogoValidate : AbstractValidator<UpdateShopLogoCommand>
{
    public UpdateShopLogoValidate()
    {
        RuleFor(a => a.LogoUrl)
            .NotEmpty()
            .WithMessage("Logo mới bắt buộc nhập.");
    }
}