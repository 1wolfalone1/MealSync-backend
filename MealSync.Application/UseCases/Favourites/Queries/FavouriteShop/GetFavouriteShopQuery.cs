using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Favourites.Queries.FavouriteShop;

public class GetFavouriteShopQuery : PaginationRequest, IQuery<Result>
{
}