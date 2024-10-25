using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCancelOrder;

public class ShopCancelOrderCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Reason { get; set; }

    public bool? IsConfirm { get; set; }
}