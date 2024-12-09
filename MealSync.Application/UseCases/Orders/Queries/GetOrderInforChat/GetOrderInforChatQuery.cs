using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.GetOrderInforNotification;

public class GetOrderInforChatQuery : IQuery<Result>
{
    public long Id { get; set; }
}