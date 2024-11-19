using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetShopDetail;

public class GetShopDetailQuery : IQuery<Result>
{
    public long ShopId { get; set; }
}