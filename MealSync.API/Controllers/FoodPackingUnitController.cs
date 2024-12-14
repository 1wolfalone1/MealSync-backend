using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Create;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Update;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.Create.ShopCreate;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.Delete;
using MealSync.Application.UseCases.FoodPackingUnits.Commands.Update.ShopUpdateFPU;
using MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUDetailForAdmin;
using MealSync.Application.UseCases.FoodPackingUnits.Queries.GetFPUForAdmin;
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

    [HttpPut(Endpoints.UPDATE_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopUpdateFoodPackingUnit([FromBody] ShopUpdateFPUCommand command, int id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.ADMIN_CREATE_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> AdminCreateFoodPackingUnit([FromBody] AdminCreateFoodPackingUnitCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.ADMIN_UPDATE_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> AdminUpdateFoodPackingUnit([FromBody] AdminUpdateFPUCommand command, int id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.ADMIN_FOOD_PACKING_UNIT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> AdminCreateFoodPackingUnit([FromQuery] GetFPUForAdminQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.ADMIN_FOOD_PACKING_UNIT_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> AdminCreateFoodPackingUnitDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetFPUDetailQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }
}