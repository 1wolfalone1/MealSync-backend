using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Orders.Queries.GetListShopStaffForShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopDeliveryStaffController : BaseApiController
{
    [HttpGet(Endpoints.GET_SHOP_DELIVER_STAFF_AVAILABLE)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> RearrangeShopCategory([FromQuery] GetListShopStaffForShopQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }
}