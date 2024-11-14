using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Payments.Commands.RePaymentOrder;
using MealSync.Application.UseCases.Payments.Queries.CheckOnlinePaymentStatus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class PaymentController : BaseApiController
{

    [HttpGet(Endpoints.GET_PAYMENT_STATUS_BY_ORDER_ID)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CheckOnlinePaymentStatusByOrderId(long id)
    {
        return HandleResult(
            await Mediator.Send(new CheckOnlinePaymentStatusQuery { Id = id }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_REPAYMENT_LINK)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetRePaymentLink(long id)
    {
        return HandleResult(
            await Mediator.Send(new RePaymentOrderCommand { OrderId = id }).ConfigureAwait(false));
    }
}