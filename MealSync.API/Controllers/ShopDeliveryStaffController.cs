using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Create;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Delete;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfo;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateStatus;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetDetailById;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetStaffForManage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopDeliveryStaffController : BaseApiController
{
    [HttpGet(Endpoints.GET_SHOP_DELIVER_STAFF_AVAILABLE)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> RearrangeShopCategory([FromQuery] GetListShopStaffForShopQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.CREATE_SHOP_DELIVERY_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateShopDeliveryStaff(CreateDeliveryStaffCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_DELIVER_STAFF_OF_SHOP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopDeliveryStaff([FromQuery] GetStaffForManageQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_DETAIL_SHOP_DELIVER_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopDeliveryStaff(long id)
    {
        return HandleResult(await Mediator.Send(new GetDetailByIdQuery { Id = id }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_INFO_SHOP_DELIVERY_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateInfoShopDeliveryStaff(UpdateDeliveryStaffCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.UPDATE_STATUS_SHOP_DELIVERY_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateStatusShopDeliveryStaff(UpdateStatusCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPut(Endpoints.DELETE_SHOP_DELIVERY_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> DeleteShopDeliveryStaff(DeleteDeliveryStaffCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}