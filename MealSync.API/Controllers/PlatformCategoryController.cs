using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.PlatformCategory.Commands.CreatePlatformCategory;
using MealSync.Application.UseCases.PlatformCategory.Commands.ReArrangePlatformCategory;
using MealSync.Application.UseCases.PlatformCategory.Commands.UpdatePlatformCategory;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.PlatformCategory.Queries.GetAll;
using MealSync.Application.UseCases.PlatformCategory.Queries.GetAllPlatformCategoryForAdmin;
using MealSync.Application.UseCases.PlatformCategory.Queries.GetPlatformCategoryDetail;
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

    [HttpPut(Endpoints.UPDATE_PLATFORM_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> UpdatePlatformCategory([FromBody] UpdatePlatformCategoryCommand command, long id)
    {
        command.Id = id;
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.REARRANGE_PLATFORM_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> ReArrangePlatformCategory([FromBody] ReArrangePlatformCategoryCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_PLATFORM_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> ReArrangePlatformCategory([FromQuery] GetAllForAdminQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_DETAIL_PLATFORM_CATEGORY)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetDetailPlatformCategory(long id)
    {
        return HandleResult(await Mediator.Send(new GetPlatformCategoryDetailQuery()
        {
            Id = id,
        }).ConfigureAwait(false));
    }
}