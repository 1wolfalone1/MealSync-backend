using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmListOrder;

public class ShopConfirmListOrderCommand : ICommand<Result>
{
    public long[] Ids { get; set; }
}