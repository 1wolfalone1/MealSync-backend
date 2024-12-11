using MealSync.Application.UseCases.Accounts.Commands.BanUnBanCustomerByMod;
using MealSync.Application.UseCases.Accounts.Queries.AdminManage.GetDetailAccount;
using MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetDetailAccount;
using MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;
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

    [HttpGet(Endpoints.MANAGE_CUSTOMER)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> GetListCustomer([FromQuery] GetListAccountQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_CUSTOMER_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> GetCustomerDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetDetailAccountQuery { AccountId = id }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.MANAGE_CUSTOMER_BAN_UNBAN)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> BanOrUnban([FromBody] BanUnBanCustomerByModCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_CUSTOMER_DETAIL_ADMIN)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetCustomerDetailForAdmin(long id)
    {
        return HandleResult(await Mediator.Send(new GetDetailAccountAdminQuery() { AccountId = id }).ConfigureAwait(false));
    }
}