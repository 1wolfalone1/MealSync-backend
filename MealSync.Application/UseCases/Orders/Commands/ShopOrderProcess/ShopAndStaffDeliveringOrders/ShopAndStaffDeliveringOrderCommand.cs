using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliveringOrders;

public class ShopAndStaffDeliveringOrderCommand : ICommand<Result>
{
    public long[] Ids { get; set; }
}