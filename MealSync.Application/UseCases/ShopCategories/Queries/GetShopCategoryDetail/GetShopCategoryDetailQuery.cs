using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategoryDetail;

public class GetShopCategoryDetailQuery : IQuery<Result>
{
    public long Id { get; set; }
}