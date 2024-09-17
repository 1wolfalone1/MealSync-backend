using MealSync.API.Attributes;
using MealSync.Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Queries;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Application.UseCases.Roles.Commands.UpdateRole;
using MealSync.Domain.Entities;

namespace MealSync.API.Controllers;

public class TestController : BaseApiController
{
    private readonly ICacheService _cacheService;

    public TestController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

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


    [HttpGet("hello")]
    [Cache]
    public async Task<IActionResult> TestCache(string hello)
    {
        return Ok("hello");
    }

    [HttpGet("delete/cache")]
    public async Task<IActionResult> TestDeleteCache()
    {
        await _cacheService.RemoveCacheResponseAsync("/api/v1/Test/hello");
        return Ok("delete");
    }
}
