using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderDetailForModerator;

public class GetOrderDetailForModeratorQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}