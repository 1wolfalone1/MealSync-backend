using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Commands.ModeratorManage.UpdateShopStatus;

public class UpdateShopStatusCommand : ICommand<Result>
{
    public long ShopId { get; set; }

    public ShopStatus Status { get; set; }

    public bool IsConfirm { get; set; }

    public string? Reason { get; set; }
}