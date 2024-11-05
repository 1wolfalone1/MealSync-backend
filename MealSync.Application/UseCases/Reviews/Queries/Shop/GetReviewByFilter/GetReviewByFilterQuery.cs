using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Queries.Shop.GetReviewByFilter;

public class GetReviewByFilterQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public RatingRanges? Rating { get; set; }

    public FilterQuery Filter { get; set; } = FilterQuery.All;

    public enum FilterQuery
    {
        All = 1,
        ContainComment = 2,
        ContainImageAndComment = 3,
    }
}