using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdatePassword;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopBanner;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopLogo;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAcceptOrderNextDays;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirmConditions;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirms;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;
using MealSync.Application.UseCases.ShopOwners.Commands.VerifyUpdateEmail;
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

    [HttpPut(Endpoints.UPDATE_SHOP_IS_ACCEPT_ORDER_NEXT_DAY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopSettingAcceptOrderNextDayShop([FromBody] UpdateShopSettingAcceptOrderNextDayCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_IS_AUTO_CONFIRM)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopSettingIsAutoConfirmShop([FromBody] UpdateShopSettingAutoConfirmCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_IS_AUTO_CONFIRM_CONDITION)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopSettingAutoConfirmConditionShop([FromBody] UpdateShopSettingAutoConfirmConditionCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.SEND_VERIFY_UPDATE_SHOP_EMAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> SendVerifyUpdateEmail([FromBody] SendVerifyUpdateEmailCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.VERIFY_UPDATE_SHOP_EMAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> VerifyUpdateEmail([FromBody] VerifyUpdateEmailCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_PASSWORD)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_BANNER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopBanner([FromBody] UpdateShopBannerCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_LOGO)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> UpdateShopLogo([FromBody] UpdateShopLogoCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}