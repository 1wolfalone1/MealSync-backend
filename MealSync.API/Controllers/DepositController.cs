using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Deposits.Commands.ShopDeposit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class DepositController : BaseApiController
{
    [HttpGet(Endpoints.CREATE_DEPOSIT_PAYMENT_URL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateDepositUrl([FromQuery] ShopDepositCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}