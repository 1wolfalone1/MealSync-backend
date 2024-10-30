using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionDetail;

public class GetPromotionDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}