using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Shops.Queries.TopShop;

public class GetTopShopHandler : IQueryHandler<GetTopShopQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetTopShopHandler(
        IShopRepository shopRepository, ICustomerBuildingRepository customerBuildingRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper
    )
    {
        _shopRepository = shopRepository;
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetTopShopQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var defaultBuilding = _customerBuildingRepository.GetDefaultByCustomerId(accountId);

        // Throw error when customer has not selected dormitory
        if (defaultBuilding == null)
        {
            throw new InvalidBusinessException(
                MessageCode.E_BUILDING_NOT_SELECT.GetDescription()
            );
        }
        else
        {
            var totalTopShops = await _shopRepository.CountTopShop(defaultBuilding.Building.DormitoryId).ConfigureAwait(false);

            if (totalTopShops == 0)
            {
                var result = new PaginationResponse<ShopSummaryResponse>(
                    new List<ShopSummaryResponse>(), totalTopShops, request.PageIndex, request.PageSize
                );

                return Result.Success(result);
            }
            else
            {
                var topShops = await _shopRepository.GetTopShop(defaultBuilding.Building.DormitoryId, request.PageIndex, request.PageSize).ConfigureAwait(false);
                var result = new PaginationResponse<ShopSummaryResponse>(
                    _mapper.Map<List<ShopSummaryResponse>>(topShops), totalTopShops, request.PageIndex, request.PageSize
                );

                return Result.Success(result);
            }
        }
    }
}