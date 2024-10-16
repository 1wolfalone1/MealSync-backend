using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Promotions.Queries.GetEligibleAndIneligiblePromotions;

public class GetEligibleAndIneligiblePromotionQuery : IQuery<Result>
{
    public long ShopId { get; set; }

    public double TotalPrice { get; set; }
}