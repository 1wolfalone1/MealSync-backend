using MealSync.API.Attributes;
using MealSync.API.Identites;
using MealSync.Application.Common.Services;
using Microsoft.AspNetCore.Mvc;
using MealSync.Application.UseCases.Accounts.Queries;
using MealSync.Application.UseCases.Orders.Queries.ShopGetQrCodeOfOrders;
using MealSync.Application.UseCases.Roles.Commands.CreateRole;
using MealSync.Application.UseCases.Roles.Commands.UpdateRole;
using MealSync.Application.UseCases.Test.Commands.TestFirebase;
using MealSync.Application.UseCases.Test.Commands.TestModeratorCreateLog;
using MealSync.Application.UseCases.Test.Commands.TestPushNotiKafkas;
using MealSync.Application.UseCases.Test.Commands.TestValidateError;
using MealSync.Application.UseCases.Test.Queries.TestError;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

public class TestController : BaseApiController
{
    private readonly ICacheService _cacheService;

    public TestController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> DefaultUrl()
    {
        return Ok(new
        {
            Version = "0.0.6",
        });
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

    [HttpPost("/api/v1/moderator/order")]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> TestModeratorLogError(TestModeratorLogCommand command)
    {
        return this.HandleResult(await this.Mediator.Send(command));
    }

    [HttpGet("/api/v1/test/order/{id:long}")]
    public async Task<IActionResult> TestShopGetQRCodeError(long id)
    {
        return this.HandleResult(await this.Mediator.Send(new ShopGetQrCodeOfOrderQuery()
        {
            Id = id,
        }));
    }

    [HttpPost("/api/v1/test/push-noti-kafka")]
    public async Task<IActionResult> TestPushNotiKafka([FromBody] TestPushNotiKafkaCommand command)
    {
        return this.HandleResult(await this.Mediator.Send(command));
    }

    [HttpPost("/api/v1/test/push-noti-firebase")]
    public async Task<IActionResult> TestPushNotiKafka([FromBody] TestFirebaseCommand command)
    {
        return this.HandleResult(await this.Mediator.Send(command));
    }
}
