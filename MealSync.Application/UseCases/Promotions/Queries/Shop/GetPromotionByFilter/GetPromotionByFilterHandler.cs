using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;

namespace MealSync.Application.UseCases.Promotions.Queries.Shop.GetPromotionByFilter;

public class GetPromotionByFilterHandler : IQueryHandler<GetPromotionByFilterQuery, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetPromotionByFilterHandler(IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetPromotionByFilterQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var data = await _promotionRepository.GetShopPromotionByFilter(
                shopId, request.SearchValue, request.Status, request.ApplyType, request.StartDate,
                request.EndDate, request.PageIndex, request.PageSize)
            .ConfigureAwait(false);

        var result = new PaginationResponse<PromotionDetailOfShop>(
            _mapper.Map<List<PromotionDetailOfShop>>(data.Promotions), data.TotalCount, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}