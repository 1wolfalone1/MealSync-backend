using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.ShopCategories.Commands.Create;
using MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopCategoryController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_SHOP_CATEGORY)]
    public async Task<IActionResult> CreateShopCategory(CreateShopCategoryCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.REARRANGE_SHOP_CATEGORY)]
    public async Task<IActionResult> RearrangeShopCategory(RearrangeShopCategoryCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}