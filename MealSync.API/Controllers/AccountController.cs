using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Accounts.Commands.CheckValidTokenJwt;
using MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.CustomerRegisterWithGoogle;
using MealSync.Application.UseCases.Accounts.Commands.CustomerLoginWithGoogle.ValidIdTokenFromFirebase;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Accounts.Commands.SendVerifyCode;
using MealSync.Application.UseCases.Accounts.Commands.ShopRegister;
using MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;
using MealSync.Application.UseCases.Accounts.Commands.UpdateDeviceToken;
using MealSync.Application.UseCases.Accounts.Commands.VerifyCode;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class AccountController : BaseApiController
{
    [HttpPost(Endpoints.LOGIN_USERNAME_PASS)]
    public async Task<IActionResult> LoginUsernamePass(LoginCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPost(Endpoints.REGISTER_CUSTOMER)]
    public async Task<IActionResult> RegisterCustomer(SignupCustomerCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }

    [HttpPost(Endpoints.SHOP_REGISTER)]
    public async Task<IActionResult> ShopRegister(ShopRegisterCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost(Endpoints.SEND_VERIFY_CODE)]
    public async Task<IActionResult> SendVerifyCode(SendVerifyCodeCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost(Endpoints.VERIFY_CODE)]
    public async Task<IActionResult> VerifyCode(VerifyCodeCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost(Endpoints.VALID_TOKEN)]
    [Authorize]
    public async Task<IActionResult> VerifyCode([FromBody] CheckValidTokenJwtCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut(Endpoints.UPDATE_DEVICE_TOKEN)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}, {IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> UpdateDeviceToken([FromBody] UpdateDeviceTokenCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost(Endpoints.LOGIN_GOOGLE)]
    public async Task<IActionResult> LoginWithGoogle([FromBody] ValidIdTokenFromFirebaseCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost(Endpoints.REGISTER_GOOGLE)]
    public async Task<IActionResult> UpdateDeviceToken([FromBody] CustomerRegisterWithGoogleCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}