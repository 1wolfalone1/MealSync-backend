using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Buildings.Queries.CheckSelection;
using MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;

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
    public async Task<IActionResult> CheckSelectedBuilding()
    {
        return HandleResult(await Mediator.Send(new CheckBuildingSelectionQuery()).ConfigureAwait(false));
    }
}