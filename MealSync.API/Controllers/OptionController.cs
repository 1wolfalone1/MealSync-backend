using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.OperatingSlots.Commands.AddShopOperatingSlots;
using MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;
using MealSync.Application.UseCases.Options.Commands.CreateNewOption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class OptionController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPost(Endpoints.CREATE_OPTION)]
    public async Task<IActionResult> AddShopProfile([FromBody] CreateNewOptionCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}