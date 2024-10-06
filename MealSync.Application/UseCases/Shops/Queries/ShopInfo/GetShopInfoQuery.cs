using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.ShopInfo;

public class GetShopInfoQuery : IQuery<Result>
{
    public long ShopId { get; set; }
}