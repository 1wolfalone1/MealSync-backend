using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopPreparingOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class DeliveryPackageController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPost(Endpoints.CREATE_DELIVERY_PACKAGE)]
    public async Task<IActionResult> AddShopProfile([FromBody] ShopCreateDeliveryPackageCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}