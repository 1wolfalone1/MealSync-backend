using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Dormitories.Queries.GetAll;
using MealSync.Application.UseCases.Dormitories.Queries.GetModDormitory;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class DormitoryController : BaseApiController
{
    [HttpGet(Endpoints.ALL_DORMITORY)]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllDormitoryQuery()).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MODERATOR_DORMITORY)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> GetAllForMod()
    {
        return HandleResult(await Mediator.Send(new GetModDormitoryQuery()).ConfigureAwait(false));
    }
}