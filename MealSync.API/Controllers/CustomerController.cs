using MealSync.Application.UseCases.Customers.Commands.UpdateAvatar;
using MealSync.Application.UseCases.Customers.Commands.UpdateProfile;

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

    [HttpPut(Endpoints.UPDATE_AVATAR)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> UpdateCustomerAvatar(IFormFile file)
    {
        return this.HandleResult(await this.Mediator.Send(new UpdateAvatarCommand
        {
            File = file,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_CUSTOMER_PROFILE)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> UpdateCustomerProfile(UpdateProfileCommand request)
    {
        return this.HandleResult(await this.Mediator.Send(request).ConfigureAwait(false));
    }
}