using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;
using MealSync.Application.UseCases.OptionGroups.Commands.LinkOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;
using MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OptionGroupController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateOptionGroup(CreateOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPost(Endpoints.LINK_FOOD_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> LinkFoodOptionGroup([FromBody] LinkOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpGet(Endpoints.GET_ALL_SHOP_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetAllShopOptionGroup([FromQuery] GetAllShopOptionGroupQuery request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.UPDATE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopOptionGroup([FromBody] UpdateOptionGroupCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }
}