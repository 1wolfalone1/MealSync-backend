using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;
using MealSync.Application.UseCases.Shops.Queries.ShopInfo;
using MealSync.Application.UseCases.Shops.Queries.TopShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_SHOP_PROFILE)]
    public async Task<IActionResult> GetShopProfile()
    {
        return HandleResult(await Mediator.Send(new GetShopConfigurationQuery()));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_SHOP_PROFILE)]
    public async Task<IActionResult> UpdateShopProfile([FromBody] UpdateShopProfileCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet(Endpoints.GET_TOP_SHOP)]
    public async Task<IActionResult> GetTopShop(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetTopShopQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_INFO)]
    public async Task<IActionResult> GetTopShop(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopInfoQuery()
        {
            ShopId = id,
        }).ConfigureAwait(false));
    }
}