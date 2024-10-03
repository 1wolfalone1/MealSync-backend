using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.AddShopOperatingSlots;
using MealSync.Application.UseCases.ShopOwners.Commands.DeleteShopOperatingSlots;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopOperatingSlots;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OperatingSlotController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPost(Endpoints.ADD_OPERATING_SLOT)]
    public async Task<IActionResult> AddShopProfile([FromBody] AddShopOperatingSlotCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_OPERATING_SLOT)]
    public async Task<IActionResult> UpdateShopProfile([FromBody] UpdateShopOperatingSlotCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpDelete(Endpoints.DELETE_OPERATING_SLOT)]
    public async Task<IActionResult> DeleteShopProfile([FromBody] DeleteShopOperatingSlotCommand command, long id)
    {
        return HandleResult(await Mediator.Send(command));
    }
}