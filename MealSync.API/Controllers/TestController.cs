using MediatR;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Queries;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Application.UseCases.Roles.Commands.UpdateRole;
using MealSync.Application.UseCases.Test.Commands.TestValidateError;
using MealSync.Application.UseCases.Test.Queries.TestError;
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

    [HttpGet("test/error")]
    public async Task<IActionResult> TestError()
    {
        return this.HandleResult(await this.Mediator.Send(new GetTestErrorQuery()));
    }

    [HttpPost("test/validate-error")]
    public async Task<IActionResult> TestValidateError(TestValidateErrorCommand command)
    {
        return this.HandleResult(await this.Mediator.Send(command));
    }
}
