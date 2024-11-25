using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reviews.Queries.GetShopReviewByOrderId;

public class GetShopReviewByOrderIdQuery : IQuery<Result>
{
    public long OrderId { get; set; }
}