using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Favourites.Queries.FavouriteShop;
using MealSync.Application.UseCases.Notifications.Commands.UpdateReadedNotification;
using MealSync.Application.UseCases.Notifications.Queries;
using MealSync.Application.UseCases.Notifications.Queries.GetNotificationForShopAndStaff;
using MealSync.Application.UseCases.Notifications.Queries.GetTotalUnreadNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class NotificationController : BaseApiController
{
    [HttpGet(Endpoints.NOTIFICATION_SHOP_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> GetNotificationShopAndStaff(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetNotificationForShopAndStaffQuery()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.TOTAL_UNREAD_NOTIFICATION_SHOP_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> GetTotalUnreadNotificationShopAndStaff(int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetTotalUnreadNotificationQuery()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.NOTIFICATION_UPDATE_SHOP_STAFF)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> UpdateReadedShopAndStaff([FromBody] UpdateReadedNotificationCommand command)
    {
        return HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }
}