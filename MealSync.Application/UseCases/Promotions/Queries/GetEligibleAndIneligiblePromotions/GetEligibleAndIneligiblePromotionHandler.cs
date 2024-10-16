using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Promotions.Queries.GetEligibleAndIneligiblePromotions;

public class GetEligibleAndIneligiblePromotionHandler : IQueryHandler<GetEligibleAndIneligiblePromotionQuery, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetEligibleAndIneligiblePromotionHandler(IPromotionRepository promotionRepository, IMapper mapper, IShopRepository shopRepository)
    {
        _promotionRepository = promotionRepository;
        _mapper = mapper;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(GetEligibleAndIneligiblePromotionQuery request, CancellationToken cancellationToken)
    {
        var data = await _promotionRepository.GetShopAvailablePromotionsByShopIdAndTotalPrice(request.ShopId, request.TotalPrice)
            .ConfigureAwait(false);
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
            return Result.Success(new
            {
                EligibleList = _mapper.Map<List<PromotionSummaryResponse>>(data.EligibleList),
                IneligibleList = _mapper.Map<List<PromotionSummaryResponse>>(data.IneligibleList),
            });
        }
    }
}