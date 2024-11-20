using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.WithdrawalRequests.Queries.GetAllWithdrawalRequestForMod;
using MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForMod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class WithdrawalRequestController : BaseApiController
{
    [HttpGet(Endpoints.MANAGE_WITHDRAWAL_REQUEST)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> WithdrawalRequestForModManage([FromQuery] GetAllWithdrawalRequestForModQuery request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_WITHDRAWAL_REQUEST_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> WithdrawalRequestDetailForModManage(long id)
    {
        return HandleResult(await Mediator.Send(new GetDetailForModQuery { WithdrawalRequestId = id }).ConfigureAwait(false));
    }
}