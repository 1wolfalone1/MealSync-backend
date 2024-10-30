using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetListShopStaffForShop;

public class GetListShopStaffForShopHandler : ICommandHandler<GetListShopStaffForShopQuery, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;
    private readonly IDapperService _dapperService;

    public GetListShopStaffForShopHandler(IAccountRepository accountRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IDapperService dapperService)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetListShopStaffForShopQuery request, CancellationToken cancellationToken)
    {
        var response = await _dapperService.SelectAsync<ShopDeliveryStaffForAssignResponse, ShopDeliveryStaffForAssignResponse.ShopStaffInforResponse, ShopDeliveryStaffForAssignResponse>(
            QueryName.GetListShopDeliveryStaffToAssign,
            (parent, child1) =>
            {
                parent.StaffInfor = child1;
                return parent;
            },
            new
            {
                // SearchText = request.SearchText,
                IntendedReceiveDate = request.IntendedReceiveDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            },
            "StaffInforSection").ConfigureAwait(false);

        return Result.Success(response.ToList());
    }
}