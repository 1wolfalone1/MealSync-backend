using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.CommissionConfigs.Commands.Create;
using MealSync.Application.UseCases.CommissionConfigs.Queries.Get;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class CommissionConfigController : BaseApiController
{
    [HttpGet(Endpoints.GET_COMMISSION_CONFIG)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetCommissionConfig()
    {
        return HandleResult(await Mediator.Send(new GetCommissionConfigQuery()).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_COMMISSION_CONFIG)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> UpdateCommissionConfig([FromBody] CreateCommissionConfigCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}