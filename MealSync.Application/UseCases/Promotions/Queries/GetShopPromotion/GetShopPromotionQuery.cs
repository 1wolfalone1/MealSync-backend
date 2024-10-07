using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Promotions.Queries.GetShopPromotion;

public class GetShopPromotionQuery : IQuery<Result>
{
    public long ShopId { get; set; }
}