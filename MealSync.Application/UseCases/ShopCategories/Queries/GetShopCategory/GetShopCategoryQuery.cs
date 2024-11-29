using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategory;

public class GetShopCategoryQuery : IQuery<Result>
{
    public long Id { get; set; }
}