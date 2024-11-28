using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reviews.Queries.GetAllReviewShopWeb;

public class GetAllReviewShopWebQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public FilterReviewShopWebMode StatusMode { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}

public enum FilterReviewShopWebMode
{
    All = 0,
    NOT_REPLY = 1,
    RELIED = 2,
    OVER_TIME_TO_REPLY = 3,
}