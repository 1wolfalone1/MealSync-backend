using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.ShopCategories.Commands.Create;
using MealSync.Application.UseCases.ShopCategories.Commands.Delete;
using MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;
using MealSync.Application.UseCases.ShopCategories.Commands.Update;
using MealSync.Application.UseCases.ShopCategories.Queries;
using MealSync.Application.UseCases.ShopCategories.Queries.GetAll;
using MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategoryDetail;
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

    [HttpPut(Endpoints.UPDATE_APP_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopCategory([FromBody] UpdateShopCategoryCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete(Endpoints.DELETE_APP_SHOP_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> DeleteShopCategory(long id)
    {
        return HandleResult(await Mediator.Send(new DeleteShopCategoryCommand()
        {
            Id = id,
        }));
    }

    [HttpGet(Endpoints.GET_APP_SHOP_CATEGORY_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopCategory(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopCategoryDetailQuery()
        {
            Id = id,
        }));
    }
}