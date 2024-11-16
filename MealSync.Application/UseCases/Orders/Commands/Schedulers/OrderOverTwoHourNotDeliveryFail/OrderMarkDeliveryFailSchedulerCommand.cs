using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Domain.Entities;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.OrderOverTwoHourNotDeliveryFail;

public class OrderMarkDeliveryFailSchedulerCommand : ICommand<BatchHistory>
{
}