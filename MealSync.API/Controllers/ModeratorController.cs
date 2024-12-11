using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Moderators.Commands.CreateModerator;
using MealSync.Application.UseCases.Moderators.Commands.UpdateModerator;
using MealSync.Application.UseCases.Moderators.Queries.GetActivityLog;
using MealSync.Application.UseCases.Moderators.Queries.GetActivityLogDetail;
using MealSync.Application.UseCases.Moderators.Queries.GetAllModerators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ModeratorController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_MODERATOR_ACCOUNT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] CreateModeratorCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet(Endpoints.GET_MODERATOR_ACCOUNT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetAllModerator([FromQuery] GetAllModeratorQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPut(Endpoints.UPDATE_MODERATOR_ACCOUNT)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> UpdateModeratorAccount([FromBody] UpdateModeratorCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet(Endpoints.GET_MODERATOR_ACTIVITY_LOG)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetModeratorAccountLog([FromQuery] GetActivityLogQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet(Endpoints.GET_MODERATOR_ACTIVITY_LOG_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetModeratorAccountLogDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetActivityLogDetailQuery()
        {
            Id = id,
        }));
    }
}