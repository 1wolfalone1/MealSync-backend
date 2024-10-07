using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Promotions.Queries.GetShopPromotion;

public class GetShopPromotionHandler : IQueryHandler<GetShopPromotionQuery, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopPromotionHandler(IPromotionRepository promotionRepository, IShopRepository shopRepository, IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopPromotionQuery request, CancellationToken cancellationToken)
    {
        var shop = _shopRepository.GetById(request.ShopId);
        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else if (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            var promotions = await _promotionRepository.GetShopAvailablePromotionsByShopId(request.ShopId).ConfigureAwait(false);
            return Result.Success(_mapper.Map<IEnumerable<PromotionSummaryResponse>>(promotions));
        }
    }
}