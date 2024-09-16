using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands;
using MealSync.Application.UseCases.Accounts.Commands.LoginPassword;
using MealSync.Application.UseCases.Accounts.Models;

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