using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;

public class ShopDeliveringOrderCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public long? ShopDeliveryStaffId { get; set; }

    public bool? IsConfirm { get; set; }
}