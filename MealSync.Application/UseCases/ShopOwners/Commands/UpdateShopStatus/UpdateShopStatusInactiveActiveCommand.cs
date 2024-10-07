using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveCommand : ICommand<Result>
{
    public int Status { get; set; }

    public bool IsConfirm { get; set; }
}