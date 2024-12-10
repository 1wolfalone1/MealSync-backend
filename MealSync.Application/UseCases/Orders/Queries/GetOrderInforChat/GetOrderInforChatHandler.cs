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

namespace MealSync.Application.UseCases.Orders.Queries.GetOrderInforNotification;

public class GetOrderInforNotificationHandler : IQueryHandler<GetOrderInforChatQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;

    public GetOrderInforNotificationHandler(IOrderRepository orderRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IAccountRepository accountRepository, IShopRepository shopRepository)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _currentAccountService = currentAccountService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(GetOrderInforChatQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var accounts = _accountRepository.GetAccountByIds(_orderRepository.GetListAccountIdRelatedToOrder(request.Id));
        var response = _mapper.Map<List<AccountInforInChatRepsonse>>(accounts);
        return Result.Success(AccountInformationChatConverter.ConvertListToDictionaryFormat(response, request.Id, _shopRepository));
    }

    private void Validate(GetOrderInforChatQuery request)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : (account.RoleId == (int) Domain.Enums.Roles.ShopDelivery ? _shopDeliveryStaffRepository.GetById(account.Id).ShopId : 0);
        var order = _orderRepository
            .Get(o => o.Id == request.Id && (o.ShopId == shopId || o.CustomerId == _currentPrincipalService.CurrentPrincipalId)).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}