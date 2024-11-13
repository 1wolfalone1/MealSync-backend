using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Payments.Commands.RePaymentOrder;

public class RePaymentOrderCommand : ICommand<Result>
{
    public long OrderId { get; set; }
}