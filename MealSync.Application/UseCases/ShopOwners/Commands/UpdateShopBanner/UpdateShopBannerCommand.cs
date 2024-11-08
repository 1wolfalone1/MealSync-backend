using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopBanner;

public class UpdateShopBannerCommand : ICommand<Result>
{
    public string BannerUrl { get; set; }
}