using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Dormitories.Queries.GetAll;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class DormitoryController : BaseApiController
{
    [HttpGet(Endpoints.ALL_DORMITORY)]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllDormitoryQuery()));
    }
}