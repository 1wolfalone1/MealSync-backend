using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.CustomerBuildings.Commands.Update;
using MealSync.Application.UseCases.CustomerBuildings.Queries.GetAllBuildingOfCustomer;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class CustomerBuildingController : BaseApiController
{
    [HttpPut(Endpoints.UPDATE_CUSTOMER_BUILDING)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> UpdateCustomerBuilding(UpdateCustomerBuildingCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_ALL_CUSTOMER_BUILDING)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetAllCustomerBuilding()
    {
        return HandleResult(await Mediator.Send(new GetAllBuildingOfCustomerQuery()).ConfigureAwait(false));
    }
}