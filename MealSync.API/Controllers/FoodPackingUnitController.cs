using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.Create.ShopCreate;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.Delete;
using MealSync.Application.UseCases.FoodPackingUnits.Queries.GetListFoodPackingUnitForShop;
using MealSync.Application.UseCases.Foods.Queries.TopFood;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class FoodPackingUnitController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopCreateFoodPackingUnit([FromBody] ShopCreateFoodPackingUnitCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopCreateFoodPackingUnit([FromQuery] GetListFoodPackingUnitForShopQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpDelete(Endpoints.DELETE_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopDeleteFoodPackingUnit(int id)
    {
        return HandleResult(await Mediator.Send(new ShopDeletePackingUnitCommand()
        {
            Id = id,
        }).ConfigureAwait(false));
    }
}