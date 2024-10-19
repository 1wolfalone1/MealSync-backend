using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopRejectOrder;

public class ShopRejectOrderCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Reason { get; set; }
}