using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;

public class GetReviewOfShopQuery : PaginationRequest, IQuery<Result>
{
    public long ShopId { get; set; }

    public ReviewFilter Filter { get; set; }

    public enum ReviewFilter
    {
        All = 1,
        ContainComment = 2,
        ContainImageAndComment = 3,
    }
}