using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
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
}