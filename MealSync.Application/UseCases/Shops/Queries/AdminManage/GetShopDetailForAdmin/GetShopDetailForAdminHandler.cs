using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetShopDetail;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Shops.Queries.AdminManage.GetShopDetailForAdmin;

public class GetShopDetailForAdminHandler : IQueryHandler<GetShopDetailForAdminQuery, Result>
{
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetShopDetailForAdminHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IShopRepository shopRepository, IMapper mapper)
    {
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopDetailForAdminQuery request, CancellationToken cancellationToken)
    {
        var dormitories = _moderatorDormitoryRepository.Get().ToList();
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var shop = await _shopRepository.GetShopManageDetail(request.ShopId, dormitoryIds).ConfigureAwait(false);
        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            var shopManageDetailResponse = _mapper.Map<ShopManageDetailResponse>(shop);
            shopManageDetailResponse.TotalRevenue = await _shopRepository.GetShopRevenue(shop.Id).ConfigureAwait(false);
            return Result.Success(shopManageDetailResponse);
        }
    }
}