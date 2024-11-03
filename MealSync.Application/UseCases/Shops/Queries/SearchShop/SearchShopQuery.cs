using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.SearchShop;

public class SearchShopQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public int? PlatformCategoryId { get; set; }

    public int? StartTime { get; set; }

    public int? EndTime { get; set; }

    public int FoodSize { get; set; } = 10;

    public OrderBy? Order { get; set; }

    public Direction Direct { get; set; } = Direction.ASC;

    public enum OrderBy
    {
        Price = 1,
        Rating = 2,
    }

    public enum Direction
    {
        ASC = 1,
        DESC = 2,
    }
}