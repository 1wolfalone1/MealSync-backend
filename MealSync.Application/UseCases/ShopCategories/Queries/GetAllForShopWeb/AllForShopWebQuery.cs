using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetAllForShopWeb;

public class AllForShopWebQuery : PaginationRequest, IQuery<Result>
{
    public string? Name { get; set; }
}