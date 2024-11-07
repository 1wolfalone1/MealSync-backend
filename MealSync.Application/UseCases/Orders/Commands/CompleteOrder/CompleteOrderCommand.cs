using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.CompleteOrder;

public class CompleteOrderCommand : ICommand<Result>
{
    public long Id { get; set; }

    public bool IsConfirm { get; set; }
}