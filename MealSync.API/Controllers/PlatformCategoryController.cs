using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.PlatformCategory.Commands.CreatePlatformCategory;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.PlatformCategory.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class PlatformCategoryController : BaseApiController
{
    [HttpGet(Endpoints.GET_ALL_PLATFORM_CATEGORY)]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllPlatformCategoryQuery()).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.CREATE_PLATFORM_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> CreatePlatform([FromBody] CreatePlatformCategoryCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}