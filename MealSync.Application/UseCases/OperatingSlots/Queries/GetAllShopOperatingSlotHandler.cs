using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OperatingSlots.Models;

namespace MealSync.Application.UseCases.OperatingSlots.Queries;

public class GetAllShopOperatingSlotHandler : IQueryHandler<GetAllShopOperatingSlotQuery, Result>
{
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IShopDeliveryStaffRepository _deliveryStaffRepository;
    private readonly IMapper _mapper;

    public GetAllShopOperatingSlotHandler(ICurrentPrincipalService currentPrincipalService, IOperatingSlotRepository operatingSlotRepository, IMapper mapper, ICurrentAccountService currentAccountService, IShopRepository shopRepository, IShopDeliveryStaffRepository deliveryStaffRepository)
    {
        _operatingSlotRepository = operatingSlotRepository;
        _mapper = mapper;
        _currentAccountService = currentAccountService;
        _deliveryStaffRepository = deliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(GetAllShopOperatingSlotQuery request, CancellationToken cancellationToken)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _deliveryStaffRepository.GetById(account.Id).ShopId;
        var operatingSlots = _operatingSlotRepository.Get(op => op.ShopId == shopId)
            .OrderBy(op => op.StartTime).ToList();
        var response = _mapper.Map<List<OperatingSlotResponse>>(operatingSlots);
        return Result.Success(response);
    }
}