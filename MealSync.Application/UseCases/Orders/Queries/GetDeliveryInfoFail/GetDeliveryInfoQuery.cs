using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryInfoFail;

public class GetDeliveryInfoQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}