using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.Orders.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopHandler : ICommandHandler<GetListShopStaffForShopQuery, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;

    public GetListShopStaffForShopHandler(IAccountRepository accountRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(GetListShopStaffForShopQuery request, CancellationToken cancellationToken)
    {
        var shopDeliveries = _shopDeliveryStaffRepository.GetListAvailableShopDeliveryStaff(request.SearchText, request.OrderByMode, _currentPrincipalService.CurrentPrincipalId.Value);
        var response = _mapper.Map<List<ShopStaffResponse>>(shopDeliveries);
        return Result.Success(response);
    }
}