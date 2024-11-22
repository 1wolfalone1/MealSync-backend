using System.Windows.Input;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopUnAssignOrder;

public class ShopUnAssignOrderCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public bool? IsConfirm { get; set; }
}