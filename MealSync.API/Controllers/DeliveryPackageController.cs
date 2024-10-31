using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Commands.UpdateDeliveryPackageGroups;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackageGroupByTimeFrames;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageGroupDetailByTimeFrames;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetListTimeFrameUnAssigns;
using MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignDeliveryPackages;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;
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

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_DELIVERY_PACKAGE_GROUP)]
    public async Task<IActionResult> AddShopProfile([FromQuery] GetDeliveryPackageGroupDetailByTimeFrameQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_TIME_FRAME_ALL_ORDER_UN_ASSIGN)]
    public async Task<IActionResult> GetTimeFrameOrderUnAssignProfile([FromQuery] GetListTimeFrameUnAssignQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.SUGGEST_CREATE_ASSIGN_ORDER)]
    public async Task<IActionResult> GetSuggestAssignOrder([FromQuery] SuggestAssignDeliveryPackageQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_ALL_DELIVERY_PACKAGE_GROUP_BY_INTERVAL)]
    public async Task<IActionResult> GetSuggestAssignOrder([FromQuery] GetAllDeliveryPackageGroupByTimeFrameQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_DELIVERY_PACKAGE_GROUP)]
    public async Task<IActionResult> GetSuggestAssignOrder([FromBody] UpdateDeliveryPackageGroupCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}