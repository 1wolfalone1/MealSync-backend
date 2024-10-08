using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.FoodDetail;

public class GetFoodDetailHandler : IQueryHandler<GetFoodDetailQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetFoodDetailHandler(IFoodRepository foodRepository, IMapper mapper, IShopRepository shopRepository)
    {
        _foodRepository = foodRepository;
        _mapper = mapper;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(GetFoodDetailQuery request, CancellationToken cancellationToken)
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
            var existed = await _foodRepository.CheckExistedAndActiveByIdAndShopId(request.FoodId, request.ShopId).ConfigureAwait(false);
            if (existed)
            {
                return Result.Success(_mapper.Map<FoodDetailResponse>(_foodRepository.GetByIdIncludeAllInfoForCustomer(request.FoodId)));
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[] { request.FoodId });
            }
        }
    }
}