using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.AdminManage.GetShopDetailForAdmin;

public class GetShopDetailForAdminQuery : IQuery<Result>
{
    public long ShopId { get; set; }
}