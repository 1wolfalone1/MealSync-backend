using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.OptionGroups.Commands.Create;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OptionGroupController : BaseApiController
{
    [HttpPost(Endpoints.CREATE_OPTION_GROUP)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateOptionGroup(CreateOptionGroupCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}