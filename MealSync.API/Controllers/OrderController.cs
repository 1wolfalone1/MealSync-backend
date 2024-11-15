using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Orders.Commands.CancelOrderCustomer;
using MealSync.Application.UseCases.Orders.Commands.CompleteOrder;
using MealSync.Application.UseCases.Orders.Commands.Create;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliveringOrders;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliveryFailOrder;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliverySuccess;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCancelOrder;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmOrder;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopPreparingOrder;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopRejectOrder;
using MealSync.Application.UseCases.Orders.Commands.ShowQRConfirm;
using MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;
using MealSync.Application.UseCases.Orders.Queries.GetDeliveryInfoFail;
using MealSync.Application.UseCases.Orders.Queries.OrderDetailCustomer;
using MealSync.Application.UseCases.Orders.Queries.OrderDetailForShop;
using MealSync.Application.UseCases.Orders.Queries.OrderHistory;
using MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;
using MealSync.Application.UseCases.Orders.Queries.ShopWebOrderByStatus;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OrderController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_ORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CreateOrder(CreateOrderCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.CANCEL_ORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CancelOrder(long id, string reason)
    {
        return HandleResult(await Mediator.Send(new CancelOrderCustomerCommand { Id = id, Reason = reason }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_IPN)]
    public async Task<IActionResult> GetIPN()
    {
        var queryParams = HttpContext.Request.Query;

        return HandleResult(await Mediator.Send(new UpdatePaymentStatusIPNCommand { Query = queryParams }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetOrderDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetOrderDetailCustomerQuery { Id = id }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_HISTORY)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetOrderHistory([FromQuery] OrderStatus[]? status, bool reviewMode, int pageIndex, int pageSize)
    {
        return HandleResult(
            await Mediator.Send(
                new GetOrderHistoryQuery { StatusList = status, ReviewMode = reviewMode, PageIndex = pageIndex, PageSize = pageSize }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_FOR_SHOP_BY_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetOrderForShopByStatus([FromQuery] GetShopOrderByStatusQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_FOR_SHOP_WEB_BY_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetOrderForShopWebByStatus([FromQuery] GetShopWebOrderByStatusQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_DETAIL_FOR_SHOP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetOrderDetailForShop(long id)
    {
        return HandleResult(await Mediator.Send(new OrderDetailForShopQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_REJECT_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopRejectOrder([FromBody] ShopRejectOrderCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_CONFIRM_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopConfirmOrder(long id)
    {
        return HandleResult(await Mediator.Send(new ShopConfirmOrderCommand()
        {
            Id = id,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_CANCEL_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopCancelOrder([FromBody] ShopCancelOrderCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_PREPARING_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopChangeToPreparingOrder([FromBody] ShopPreparingOrderCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_ASSIGN_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopAssignOrder([FromBody] ShopAssignOrderCommand command, long id)
    {
        command.OrderId = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_DELIVERED_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> ShopChangeToDeliveredOrder([FromBody] ShopAndStaffDeliverySuccessCommand command, long id)
    {
        command.OrderRequestId = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_STAFF_DELIVERED_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> ShopAndStaffChangeToDeliveredOrder([FromBody] ShopAndStaffDeliverySuccessCommand command, long id)
    {
        command.OrderRequestId = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_DELIVERED_FAIL_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> ShopChangeToDeliveredOrder([FromBody] ShopAndStaffDeliveryFailOrderCommand command, long id)
    {
        command.OrderId = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_DELIVERING_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> ShopChangeToDeliveringOrder([FromBody] ShopAndStaffDeliveringOrderCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SHOP_STAFF_DELIVERING_ORDER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> ShopAndStaffChangeToDeliveringOrder([FromBody] ShopAndStaffDeliveringOrderCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SHOW_QR_FOR_CONFIRM)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> ShowQrConfirm(long id)
    {
        return HandleResult(await Mediator.Send(new ShowQRConfirmCommand { Id = id }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.COMPLETED_ORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CompletedOrder([FromBody] CompleteOrderCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SHOP_DELIVERED_INFOR_EVIDENCE)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> ShopChangeToDeliveredOrder(long id)
    {
        return HandleResult(await Mediator.Send(new GetDeliveryInfoQuery()
        {
            OrderId = id,
        }).ConfigureAwait(false));
    }
}