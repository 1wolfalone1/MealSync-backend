using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveCommand : ICommand<Result>
{
    public ShopStatus Status { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public bool IsConfirm { get; set; }
}