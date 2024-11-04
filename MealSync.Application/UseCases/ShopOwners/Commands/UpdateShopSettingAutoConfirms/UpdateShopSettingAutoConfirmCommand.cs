using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirms;

public class UpdateShopSettingAutoConfirmCommand : ICommand<Result>
{
    public bool IsAutoOrderConfirmation { get; set; }
}