using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Buildings.Queries.CheckSelection;
using MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class BuildingController : BaseApiController
{
    [HttpGet(Endpoints.GET_BUILDING_BY_DORMITORY)]
    public async Task<IActionResult> GetBuildingByDormitory(int id, string query)
    {
        return HandleResult(await Mediator.Send(new GetByDormitoryIdQuery
        {
            DormitoryId = id,
            Query = query
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.CHECK_BUILDING_SELECTION)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> CheckSelectedBuilding()
    {
        return HandleResult(await Mediator.Send(new CheckBuildingSelectionQuery()).ConfigureAwait(false));
    }
}