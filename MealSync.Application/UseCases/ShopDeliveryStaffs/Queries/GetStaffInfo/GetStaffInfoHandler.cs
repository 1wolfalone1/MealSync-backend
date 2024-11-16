using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetStaffInfo;

public class GetStaffInfoHandler : IQueryHandler<GetStaffInfoQuery, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetStaffInfoHandler(IShopDeliveryStaffRepository shopDeliveryStaffRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetStaffInfoQuery request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdIncludeAccount(accountId).ConfigureAwait(false);
        return Result.Success(_mapper.Map<ShopDeliveryStaffInfoResponse>(shopDeliveryStaff));
    }
}