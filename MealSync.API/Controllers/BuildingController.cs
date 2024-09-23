using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Buildings.Queries.GetByDormitoryId;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class BuildingController : BaseApiController
{
    [HttpGet(Endpoints.GET_BUILDING_BY_DORMITORY)]
    public async Task<IActionResult> LoginUsernamePass(int id, string query)
    {
        return HandleResult(await Mediator.Send(new GetByDormitoryIdQuery
        {
            DormitoryId = id,
            Query = query
        }));
    }
}