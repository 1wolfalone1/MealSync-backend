using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Shops.Queries.ShopInfo;

public class GetShopInfoHandler : IQueryHandler<GetShopInfoQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopInfoHandler(IShopRepository shopRepository, IMapper mapper)
    {
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopInfoQuery request, CancellationToken cancellationToken)
    {
        var shop = await _shopRepository.GetShopInfoById(request.ShopId).ConfigureAwait(false);

        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            if (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned)
            {
                throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { request.ShopId });
            }
            else
            {
                return Result.Success(_mapper.Map<ShopInfoResponse>(shop));
            }
        }
    }
}