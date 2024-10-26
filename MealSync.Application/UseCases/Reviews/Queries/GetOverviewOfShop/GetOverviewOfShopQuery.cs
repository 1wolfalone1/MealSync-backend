using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reviews.Queries.GetOverviewOfShop;

public class GetOverviewOfShopQuery : IQuery<Result>
{
    public long ShopId { get; set; }
}