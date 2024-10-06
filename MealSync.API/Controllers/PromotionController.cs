using MealSync.API.Shared;
using MealSync.Application.UseCases.Promotions.Queries.GetShopPromotion;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Microsoft.AspNetCore.Components.Route(Endpoints.BASE)]
public class PromotionController : BaseApiController
{
    [HttpGet(Endpoints.GET_SHOP_PROMOTION)]
    public async Task<IActionResult> GetShopPromotion(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopPromotionQuery()
        {
            ShopId = id,
        }).ConfigureAwait(false));
    }
}