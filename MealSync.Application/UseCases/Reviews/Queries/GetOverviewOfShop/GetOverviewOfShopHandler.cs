using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reviews.Queries.GetOverviewOfShop;

public class GetOverviewOfShopHandler : IQueryHandler<GetOverviewOfShopQuery, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IShopRepository _shopRepository;

    public GetOverviewOfShopHandler(IReviewRepository reviewRepository, IShopRepository shopRepository)
    {
        _reviewRepository = reviewRepository;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(GetOverviewOfShopQuery request, CancellationToken cancellationToken)
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
            return Result.Success(_reviewRepository.GetReviewOverviewByShopId(request.ShopId));
        }
    }
}