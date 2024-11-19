using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetListShop;

public class GetListShopQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public ShopStatus? Status { get; set; }

    public FilterShopOrderBy OrderBy { get; set; } = FilterShopOrderBy.CreatedDate;

    public FilterShopDirection Direction { get; set; } = FilterShopDirection.DESC;

    public enum FilterShopOrderBy
    {
        CreatedDate = 1,
        ShopName = 2,
        ShopOwnerName = 3,
        Revenue = 4,
        TotalOrder = 5,
        TotalFood = 6,
    }

    public enum FilterShopDirection
    {
        ASC = 1,
        DESC = 2,
    }
}