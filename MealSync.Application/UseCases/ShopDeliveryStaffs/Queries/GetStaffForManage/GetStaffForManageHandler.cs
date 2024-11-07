using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetStaffForManage;

public class GetStaffForManageHandler : IQueryHandler<GetStaffForManageQuery, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetStaffForManageHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetStaffForManageQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var data = await _shopDeliveryStaffRepository.GetAllStaffOfShop(
                shopId, request.SearchValue, request.Status, request.PageIndex, request.PageSize)
            .ConfigureAwait(false);
        var result = new PaginationResponse<ShopDeliveryStaffInfoResponse>(
            _mapper.Map<List<ShopDeliveryStaffInfoResponse>>(data.ShopDeliveryStaffs), data.TotalCounts, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}