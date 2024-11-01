using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Accounts.Commands.SendVerifyCode;
using MealSync.Application.UseCases.Accounts.Commands.ShopRegister;
using MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;
using MealSync.Application.UseCases.Accounts.Commands.VerifyCode;

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
}