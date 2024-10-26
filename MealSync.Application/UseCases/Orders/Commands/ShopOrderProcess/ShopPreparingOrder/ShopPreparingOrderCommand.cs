using System.Windows.Input;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopPreparingOrder;

public class ShopPreparingOrderCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool? IsConfirm { get; set; }
}