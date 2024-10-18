using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Orders.Commands.CancelOrderCustomer;
using MealSync.Application.UseCases.Orders.Commands.Create;
using MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;
using MealSync.Application.UseCases.Orders.Queries.OrderDetailCustomer;
using MealSync.Application.UseCases.Orders.Queries.OrderDetailForShop;
using MealSync.Application.UseCases.Orders.Queries.OrderHistory;
using MealSync.Application.UseCases.Orders.Queries.ShopOrderByStatus;
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
    // [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CancelOrder(long id)
    {
        return HandleResult(await Mediator.Send(new CancelOrderCustomerCommand { Id = id }).ConfigureAwait(false));
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
    public async Task<IActionResult> GetOrderHistory([FromQuery] List<OrderStatus>? status, int pageIndex, int pageSize)
    {
        return HandleResult(
            await Mediator.Send(
                new GetOrderHistoryQuery { StatusList = status, PageIndex = pageIndex, PageSize = pageSize }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_FOR_SHOP_BY_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetOrderForShopByStatus([FromQuery] GetShopOrderByStatusQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ORDER_DETAIL_FOR_SHOP_BY_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetOrderDetailForShopByStatus(long id)
    {
        return HandleResult(await Mediator.Send(new OrderDetailForShopQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }
}