using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopStatistics;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopStatisticSummary;
using MealSync.Application.UseCases.Shops.Queries.SearchShop;
using MealSync.Application.UseCases.Shops.Queries.ShopInfo;
using MealSync.Application.UseCases.Shops.Queries.TopShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ShopController : BaseApiController
{
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpGet(Endpoints.GET_SHOP_PROFILE)]
    public async Task<IActionResult> GetShopProfile()
    {
        return HandleResult(await Mediator.Send(new GetShopConfigurationQuery()));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_SHOP_PROFILE)]
    public async Task<IActionResult> UpdateShopProfile([FromBody] UpdateShopProfileCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet(Endpoints.GET_TOP_SHOP)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetTopShop(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetTopShopQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    [HttpPut(Endpoints.UPDATE_SHOP_ACTIVE_INACTIVE)]
    public async Task<IActionResult> UpdateShopStatusActiveInactive([FromBody] UpdateShopStatusInactiveActiveCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_INFO)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetTopShop(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopInfoQuery()
        {
            ShopId = id,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SEARCH_SHOP)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> SearchShop([FromQuery] SearchShopQuery searchShopQuery)
    {
        return HandleResult(await Mediator.Send(searchShopQuery).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_STATISTICS)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopStatistics([FromQuery] ShopStatisticQuery searchShopQuery)
    {
        return HandleResult(await Mediator.Send(searchShopQuery).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOP_STATISTICS_SUMMARY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopStatisticsSummary()
    {
        return HandleResult(await Mediator.Send(new ShopStatisticSummaryQuery()).ConfigureAwait(false));
    }
}