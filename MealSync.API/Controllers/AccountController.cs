using MealSync.API.Shared;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class AccountController : BaseApiController
{
    [HttpPost(Endpoints.LOGIN_USERNAME_PASS)]
    public async Task<IActionResult> LoginUsernamePass(LoginCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}