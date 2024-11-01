using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Create;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;
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

    [HttpPost(Endpoints.CREATE_SHOP_DELIVERY_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateShopDeliveryStaff(CreateDeliveryStaffCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}