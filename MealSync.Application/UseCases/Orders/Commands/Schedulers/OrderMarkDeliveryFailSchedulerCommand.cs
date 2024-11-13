using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers;

public class OrderMarkDeliveryFailSchedulerCommand : ICommand<BatchHistory>
{
}