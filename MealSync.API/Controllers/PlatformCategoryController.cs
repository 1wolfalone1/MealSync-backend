using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.PlatformCategory.Queries.GetAll;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class PlatformCategoryController : BaseApiController
{
    [HttpGet(Endpoints.GET_ALL_PLATFORM_CATEGORY)]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllPlatformCategoryQuery()).ConfigureAwait(false));
    }
}