using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Accounts.Commands.ShopRegister;
using MealSync.Application.UseCases.Accounts.Commands.SignupCustomer;

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
}