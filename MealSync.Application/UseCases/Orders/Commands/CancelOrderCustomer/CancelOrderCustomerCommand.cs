using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.CancelOrderCustomer;

public class CancelOrderCustomerCommand : ICommand<Result>
{
    public long Id { get; set; }
}