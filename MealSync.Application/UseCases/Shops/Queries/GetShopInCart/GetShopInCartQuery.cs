using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.GetShopInCart;

public class GetShopInCartQuery : IQuery<Result>
{
    public List<long> Ids { get; set; }
}