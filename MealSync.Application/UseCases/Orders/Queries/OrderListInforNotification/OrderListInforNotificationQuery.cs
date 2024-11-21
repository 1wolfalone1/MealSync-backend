using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.OrderListInforNotification;

public class OrderListInforNotificationQuery : IQuery<Result>
{
    public long[] Ids { get; set; }
}