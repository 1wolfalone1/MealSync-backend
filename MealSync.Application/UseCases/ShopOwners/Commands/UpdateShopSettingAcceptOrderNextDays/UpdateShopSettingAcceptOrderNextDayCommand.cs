using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAcceptOrderNextDays;

public class UpdateShopSettingAcceptOrderNextDayCommand : ICommand<Result>
{
    public bool IsAcceptingOrderNextDay { get; set; }
}