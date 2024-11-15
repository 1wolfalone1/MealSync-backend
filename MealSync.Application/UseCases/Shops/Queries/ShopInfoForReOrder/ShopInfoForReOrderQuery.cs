using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.ShopInfoForReOrder;

public class ShopInfoForReOrderQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}