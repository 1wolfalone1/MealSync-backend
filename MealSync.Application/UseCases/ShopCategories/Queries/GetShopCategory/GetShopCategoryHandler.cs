using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.ShopCategories.Queries.GetShopCategory;

public class GetShopCategoryHandler : IQueryHandler<GetShopCategoryQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IMapper _mapper;

    public GetShopCategoryHandler(IShopRepository shopRepository, IShopCategoryRepository shopCategoryRepository, IMapper mapper)
    {
        _shopRepository = shopRepository;
        _shopCategoryRepository = shopCategoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopCategoryQuery request, CancellationToken cancellationToken)
    {
        var shop = _shopRepository.GetById(request.Id);
        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else if (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { request.Id });
        }
        else
        {
            var shopCategories = _shopCategoryRepository.GetAllByShopId(request.Id);
            var response = _mapper.Map<List<ShopCategoryResponse>>(shopCategories);
            return Result.Success(response);
        }
    }
}