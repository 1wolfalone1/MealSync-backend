using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirmConditions;

public class UpdateShopSettingAutoConfirmConditionCommand : ICommand<Result>
{
    public bool? IsAutoOrderConfirmation { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int MinOrderHoursInAdvance { get; set; }
}