using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Shops.Queries.TopShop;

public class GetTopShopQuery : PaginationRequest, IQuery<Result>
{
}