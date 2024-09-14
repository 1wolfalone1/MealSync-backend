using MediatR;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Queries;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Application.UseCases.Roles.Commands.UpdateRole;
using MealSync.Domain.Entities;

namespace MealSync.API.Controllers;

public class TestController : BaseApiController
{
    [HttpGet("hehe")]
    public async Task<IActionResult> GetAccount()
    {
        return this.HandleResult(await this.Mediator.Send(new GetAllAccountQuery()));
    }

    [HttpPost("role")]
    public async Task<IActionResult> CreateRole([FromBody] string name)
    { 
        return this.HandleResult(await this.Mediator.Send(new CreateRoleCommand { Name = name}));
    }

    [HttpPut("role")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRole role)
    {
        return this.HandleResult(await this.Mediator.Send(new UpdateRoleCommand { Role = role }));
    }
}
