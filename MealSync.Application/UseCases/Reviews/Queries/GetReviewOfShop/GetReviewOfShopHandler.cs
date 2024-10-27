using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;

public class GetReviewOfShopHandler : IQueryHandler<GetReviewOfShopQuery, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;

    public GetReviewOfShopHandler(IReviewRepository reviewRepository, IShopRepository shopRepository, IOrderDetailRepository orderDetailRepository)
    {
        _reviewRepository = reviewRepository;
        _shopRepository = shopRepository;
        _orderDetailRepository = orderDetailRepository;
    }

    public async Task<Result<Result>> Handle(GetReviewOfShopQuery request, CancellationToken cancellationToken)
    {
        var shop = _shopRepository.GetById(request.ShopId);

        if (shop == default || shop.Status == ShopStatus.Deleted || shop.Status == ShopStatus.UnApprove)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else if (shop.Status == ShopStatus.Banning && shop.Status == ShopStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            var data = await _reviewRepository.GetByShopId(request.ShopId, request.Filter, request.PageIndex, request.PageSize).ConfigureAwait(false);
            foreach (var review in data.Reviews)
            {
                review.Description = await _orderDetailRepository.GetOrderDescriptionByOrderId(review.OrderId).ConfigureAwait(false);
            }

            var result = new PaginationResponse<ReviewShopDto>(data.Reviews, data.TotalCount, request.PageIndex, request.PageSize);

            return Result.Success(result);
        }
    }
}