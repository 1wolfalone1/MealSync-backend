using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.OperatingSlots.Commands.AddShopOperatingSlots;
using MealSync.Application.UseCases.OperatingSlots.Commands.DeleteShopOperatingSlots;
using MealSync.Application.UseCases.OperatingSlots.Commands.UpdateShopOperatingSlots;
using MealSync.Application.UseCases.OperatingSlots.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OperatingSlotController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPost(Endpoints.ADD_OPERATING_SLOT)]
    public async Task<IActionResult> AddOperatingSlot([FromBody] AddShopOperatingSlotCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_OPERATING_SLOT)]
    public async Task<IActionResult> UpdateOperatingSlot([FromBody] UpdateShopOperatingSlotCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpDelete(Endpoints.DELETE_OPERATING_SLOT)]
    public async Task<IActionResult> DeleteOperatingSlot([FromBody] DeleteShopOperatingSlotCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_ALL_OPERATING_SLOT)]
    public async Task<IActionResult> GetAllOperatingSlote()
    {
        return HandleResult(await Mediator.Send(new GetAllShopOperatingSlotQuery()));
    }
}