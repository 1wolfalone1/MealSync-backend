using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.SendVerifyUpdateEmail;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateAvatar;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateEmail;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdatePassword;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopBanner;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopLogo;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopProfile;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAcceptOrderNextDays;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirmConditions;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirms;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;
using MealSync.Application.UseCases.ShopOwners.Commands.VerifyAccountForUpdate;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopConfigurations;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopStatistics;
using MealSync.Application.UseCases.ShopOwners.Queries.ShopStatisticSummary;
using MealSync.Application.UseCases.Shops.Commands.ModeratorManage.UpdateShopStatus;
using MealSync.Application.UseCases.Shops.Queries.AdminManage.GetShopDetailForAdmin;
using MealSync.Application.UseCases.Shops.Queries.GetMaxCarryWeightOfShop;
using MealSync.Application.UseCases.Shops.Queries.GetShopInCart;
using MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetListShop;
using MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetShopDetail;
using MealSync.Application.UseCases.Shops.Queries.SearchShop;
using MealSync.Application.UseCases.Shops.Queries.ShopInfo;
using MealSync.Application.UseCases.Shops.Queries.ShopInfoForReOrder;
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
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> SendVerifyUpdateEmail([FromBody] SendVerifyUpdateEmailCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.VERIFY_OLD_EMAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> VerifyOldEmail([FromBody] VerifyAccountForUpdateCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_EMAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.UPDATE_SHOP_PASSWORD)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
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

    [HttpPut(Endpoints.UPDATE_SHOP_AVATAR)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> UpdateShopAvatar([FromForm] UpdateAvatarCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SHOP_INFO_REORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> ShopInfoReOrder(long id)
    {
        return HandleResult(await Mediator.Send(new ShopInfoForReOrderQuery { OrderId = id }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_SHOP)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> ManageShop([FromQuery] GetListShopQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_SHOP_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> GetShopDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopDetailQuery { ShopId = id }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.MANAGE_SHOP_UPDATE_STATUS)]
    [Authorize(Roles = $"{IdentityConst.ModeratorClaimName}")]
    public async Task<IActionResult> UpdateShopStatus([FromBody] UpdateShopStatusCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.SHOP_CART_INFO)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetShopInCart([FromQuery] GetShopInCartQuery query)
    {
        return HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_SHOPM_MAX_CARRY_WEIGHT)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopMaxCarryWeight()
    {
        return HandleResult(await Mediator.Send(new GetMaxCarryWeightOfShopQuery()).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.MANAGE_SHOP_DETAIL_ADMIN)]
    [Authorize(Roles = $"{IdentityConst.AdminClaimName}")]
    public async Task<IActionResult> GetShopDetailForAdmin(long id)
    {
        return HandleResult(await Mediator.Send(new GetShopDetailForAdminQuery { ShopId = id }).ConfigureAwait(false));
    }
}