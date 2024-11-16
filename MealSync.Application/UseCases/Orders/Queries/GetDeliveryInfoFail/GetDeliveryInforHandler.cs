using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryInfoFail;

public class GetDeliveryInforHandler : IQueryHandler<GetDeliveryInfoQuery, Result>
{
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public GetDeliveryInforHandler(IMapper mapper, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _mapper = mapper;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _currentAccountService = currentAccountService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(GetDeliveryInfoQuery request, CancellationToken cancellationToken)
    {
        // Vallidate
        Validate(request);

        var order = _orderRepository.GetById(request.OrderId);
        var evidence = _mapper.Map<EvidenceOrderResponse>(order);
        var response = _mapper.Map<DeliveryInforResponse>(order);
        response.DeliveryFaileEvidence = evidence;
        return Result.Success(response);
    }

    private void Validate(GetDeliveryInfoQuery request)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        if (_orderRepository.Get(o => o.Id == request.OrderId && o.ShopId == shopId).SingleOrDefault() == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);
    }
}