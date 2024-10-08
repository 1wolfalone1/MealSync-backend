using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Foods.Commands.Create;
using MealSync.Application.UseCases.Foods.Commands.Update;
using MealSync.Application.UseCases.Foods.Queries.FoodDetail;
using MealSync.Application.UseCases.Foods.Queries.GetAll;
using MealSync.Application.UseCases.Foods.Queries.ShopFood;
using MealSync.Application.UseCases.Foods.Queries.TopFood;
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
}