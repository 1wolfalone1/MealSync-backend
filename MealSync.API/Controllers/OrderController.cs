using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.UseCases.Orders.Commands.Create;
using MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;
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

    [HttpGet(Endpoints.GET_IPN)]
    public async Task<IActionResult> GetIPN()
    {
        var queryParams = HttpContext.Request.Query;

        return HandleResult(await Mediator.Send(new UpdatePaymentStatusIPNCommand { Query = queryParams }).ConfigureAwait(false));
    }
}