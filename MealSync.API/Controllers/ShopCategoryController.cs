using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.ShopCategories.Commands.Create;
using MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;
using MealSync.Application.UseCases.ShopCategories.Queries;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopCategoryController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateShopCategory(CreateShopCategoryCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.REARRANGE_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> RearrangeShopCategory(RearrangeShopCategoryCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpGet(Endpoints.GET_APP_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetAllShopCategory()
    {
        return HandleResult(await Mediator.Send(new GetAllShopCategoryQuery()));
    }
}