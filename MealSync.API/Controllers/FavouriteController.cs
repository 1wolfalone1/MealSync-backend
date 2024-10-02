using MealSync.API.Shared;
using MealSync.Application.UseCases.Favourites.Commands.MarkFavourite;
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
}