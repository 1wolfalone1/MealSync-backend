using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Exceptions.Base;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Queries.GetDetailById;

public class GetDetailByIdHandler : IQueryHandler<GetDetailByIdQuery, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetDetailByIdHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);
        if (shopDeliveryStaff == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            return Result.Success(_mapper.Map<ShopDeliveryStaffInfoResponse>(shopDeliveryStaff));
        }
    }
}