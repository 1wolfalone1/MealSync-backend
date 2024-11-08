using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopLogo;

public class UpdateShopLogoCommand : ICommand<Result>
{
    public string LogoUrl { get; set; }
}