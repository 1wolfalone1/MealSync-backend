using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Reviews.Commands.ReviewOrderOfCustomer;
using MealSync.Application.UseCases.Reviews.Commands.ShopReplyReviewOfCustomers;
using MealSync.Application.UseCases.Reviews.Queries.GetOverviewOfShop;
using MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;
using MealSync.Application.UseCases.Reviews.Queries.GetShopReviewByOrderId;
using MealSync.Application.UseCases.Reviews.Queries.Shop.GetReviewByFilter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class ReviewController : BaseApiController
{
    [HttpPost(Endpoints.REVIEW_ORDER)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> ReviewOrderOfCustomer(ReviewOrderOfCustomerCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.REVIEW_OF_SHOP)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetReviewOfShop(long shopId, GetReviewOfShopQuery.ReviewFilter filter, int pageIndex, int pageSize)
    {
        return HandleResult(await Mediator.Send(new GetReviewOfShopQuery
        {
            ShopId = shopId,
            Filter = filter,
            PageIndex = pageIndex,
            PageSize = pageSize,
        }).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.REVIEW_SUMMARY_OF_SHOP)]
    [Authorize(Roles = $"{IdentityConst.CustomerClaimName}")]
    public async Task<IActionResult> GetReviewSummaryOfShop(int shopId)
    {
        return this.HandleResult(await Mediator.Send(new GetOverviewOfShopQuery
        {
            ShopId = shopId,
        }).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.CREATE_REVIEW_OF_SHOP_OWNER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CreateReviewOfShopOwner([FromBody] ShopReplyReviewOfCustomerCommand command)
    {
        return this.HandleResult(await Mediator.Send(command).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_REVIEW_OF_SHOP_OWNER)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetReviewOfShopOwner([FromQuery] GetReviewByFilterQuery query)
    {
        return this.HandleResult(await Mediator.Send(query).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_REVIEW_BASE_ON_ORDER_ID)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}, {IdentityConst.ShopDeliveryClaimName}")]
    public async Task<IActionResult> GetReviewOfOrderId(int id)
    {
        return this.HandleResult(await Mediator.Send(new GetShopReviewByOrderIdQuery()
        {
            OrderId = id,
        }).ConfigureAwait(false));
    }
}