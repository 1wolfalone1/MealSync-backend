using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.OptionGroups.Commands.Create;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OptionGroupController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_OPTION_GROUP)]
    public async Task<IActionResult> CreateOptionGroup(CreateOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}