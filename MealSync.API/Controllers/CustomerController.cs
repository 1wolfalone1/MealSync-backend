namespace MealSync.API.Controllers;

using MealSync.API.Identites;
using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MealSync.Application.UseCases.Customers.Queries.GetCustomerInfo;

[Route(Endpoints.BASE)]
public class CustomerController : BaseApiController
{
    [HttpGet(Endpoints.GET_CUSTOMER_INFO)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetCustomerInfo()
    {
        return HandleResult(await Mediator.Send(new GetCustomerInfoQuery()).ConfigureAwait(false));
    }
}