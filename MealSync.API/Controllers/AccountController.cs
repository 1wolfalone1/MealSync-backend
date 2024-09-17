using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;

namespace MealSync.API.Controllers;

[Route("/api/v1/")]
public class AccountController : BaseApiController
{
    [HttpPost("customer/login")]
    public async  Task<IActionResult> Login(LoginCommand request)
    {
        return HandleResult(await Mediator.Send(request));
    }
}