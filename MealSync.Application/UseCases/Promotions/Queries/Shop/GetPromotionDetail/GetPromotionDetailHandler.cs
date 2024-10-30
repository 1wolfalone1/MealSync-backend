using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionDetail;

public class GetPromotionDetailHandler : IQueryHandler<GetPromotionDetailQuery, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetPromotionDetailHandler(IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetPromotionDetailQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var promotion = await _promotionRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (promotion == default || promotion.Status == PromotionStatus.Delete)
        {
            throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            return Result.Success(_mapper.Map<PromotionDetailOfShop>(promotion));
        }
    }
}