using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;
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
}