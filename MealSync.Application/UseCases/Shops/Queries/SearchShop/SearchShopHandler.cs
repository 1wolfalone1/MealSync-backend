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

namespace MealSync.Application.UseCases.Shops.Queries.SearchShop;

public class SearchShopHandler : IQueryHandler<SearchShopQuery, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public SearchShopHandler(
        IShopRepository shopRepository, ICustomerBuildingRepository customerBuildingRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _shopRepository = shopRepository;
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(SearchShopQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var customerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(customerId);

        if (customerBuilding == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_BUILDING_NOT_SELECT.GetDescription()
            );
        }
        else
        {
            var dormitoryId = customerBuilding.Building.DormitoryId;
            var data = await _shopRepository.SearchShops(
                    dormitoryId, request.SearchValue, request.PlatformCategoryId, request.StartTime, request.EndTime,
                    request.FoodSize, request.Order, request.Direct, request.PageIndex, request.PageSize)
                .ConfigureAwait(false);

            var result = new PaginationResponse<SearchShopResponse>(
                _mapper.Map<List<SearchShopResponse>>(data.Shops), data.TotalCounts, request.PageIndex, request.PageSize);
            return Result.Success(result);
        }
    }
}