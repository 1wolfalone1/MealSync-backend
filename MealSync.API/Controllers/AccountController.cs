using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Commands;
using MealSync.Application.UseCases.Accounts.Models;

namespace MealSync.API.Controllers;

[Route("/api/v1/")]
public class AccountController : BaseApiController
{
    [HttpPost("customer/login")]
    public async  Task<IActionResult> Login(AccountLoginRequest loginRequest)
    {
        return this.HandleResult(await this.Mediator.Send(new CustomerLoginCommand
        {
            AccountLogin = loginRequest
        }));
    }
}