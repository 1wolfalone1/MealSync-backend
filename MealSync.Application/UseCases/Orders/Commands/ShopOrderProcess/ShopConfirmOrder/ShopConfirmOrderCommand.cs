using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmOrder;

public class ShopConfirmOrderCommand : ICommand<Result>
{
    public long Id { get; set; }
}