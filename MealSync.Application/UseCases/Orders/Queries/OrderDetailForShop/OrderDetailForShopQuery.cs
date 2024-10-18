using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailForShop;

public class OrderDetailForShopQuery : IQuery<Result>
{
    public long Id { get; set; }
}