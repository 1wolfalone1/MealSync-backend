using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Foods.Commands.Create;
using MealSync.Application.UseCases.Foods.Commands.Delete;
using MealSync.Application.UseCases.Foods.Commands.LinkWithListOptionGroups;
using MealSync.Application.UseCases.Foods.Commands.ShopUpdateFoodStatus;
using MealSync.Application.UseCases.Foods.Commands.Update;
using MealSync.Application.UseCases.Foods.Queries.FoodDetail;
using MealSync.Application.UseCases.Foods.Queries.FoodDetailOfShop;
using MealSync.Application.UseCases.Foods.Queries.GetAll;
using MealSync.Application.UseCases.Foods.Queries.GetByIds;
using MealSync.Application.UseCases.Foods.Queries.ShopFood;
using MealSync.Application.UseCases.Foods.Queries.ShopOwnerFood;
using MealSync.Application.UseCases.Foods.Queries.TopFood;
using MealSync.Application.UseCases.Foods.Queries.Web.GetAllShopFood;
using MealSync.Application.UseCases.ShopCategories.Commands.LinkFoods;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class FoodController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_FOOD)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateFood(CreateFoodCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpGet(Endpoints.GET_TOP_FOOD)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetTopFood(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetTopFoodQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_FOOD)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetShopFoodDivideByCategory(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopFoodQuery
        {
            ShopId = id,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ALL_SHOP_FOOD)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetAllShopFood(long id, int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetAllShopFoodQuery
        {
            ShopId = id,
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_FOOD_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetFoodDetail(long foodId, long shopId)
    {
        return HandleResult(await Mediator.Send(new GetFoodDetailQuery
        {
            FoodId = foodId,
            ShopId = shopId,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_FOOD)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateFood(UpdateFoodCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPost(Endpoints.GET_FOOD_BY_IDS)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetFoodInCart(GetByIdsForCartQuery request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_FOOD)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopOwnerFood([FromQuery] GetShopOwnerFoodQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet(Endpoints.GET_SHOP_FOOD_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetFoodInCart(long id)
    {
        return HandleResult(await Mediator.Send(new GetFoodDetailOfShopQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_FOOD_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopOwnerUpdateFoodStatus([FromBody] ShopUpdateFoodStatusCommand command,long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpDelete(Endpoints.DELETE_FOOD_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopOwnerUpdateFoodStatus(long id)
    {
        return HandleResult(await Mediator.Send(new DeleteFoodCommand()
        {
            Id = id,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_FOOD_FOR_WEB)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetAllShopFoodForWeb ([FromQuery] AllShopFoodQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.FOOD_LINK_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> LinkFoodToShopCategory ([FromBody] LinkFoodCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.FOOD_LINK_OPTION_GROUPS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> LinkFoodToOptionGroups ([FromBody] LinkWithListOptionGroupCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}