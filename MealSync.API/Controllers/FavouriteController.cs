using MealSync.API.Shared;
using MealSync.Application.UseCases.Favourites.Commands.MarkFavourite;
using MealSync.Application.UseCases.Favourites.Queries.FavouriteShop;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class FavouriteController : BaseApiController
{
    [HttpPut(Endpoints.FAVOURITE_SHOP)]
    public async Task<IActionResult> FavouriteShop(long id)
    {
        return HandleResult(await Mediator.Send(new MarkFavouriteCommand { ShopId = id }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_FAVOURITE_SHOP)]
    public async Task<IActionResult> GetFavouriteShop(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetFavouriteShopQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }
}