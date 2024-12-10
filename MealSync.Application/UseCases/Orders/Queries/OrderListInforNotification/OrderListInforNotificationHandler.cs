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

namespace MealSync.Application.UseCases.Orders.Queries.OrderListInforNotification;

public class OrderListInforNotificationHandler : IQueryHandler<OrderListInforNotificationQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;

    public OrderListInforNotificationHandler(IOrderRepository orderRepository, ICurrentAccountService currentAccountService, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IMapper mapper, IAccountRepository accountRepository, IShopRepository shopRepository)
    {
        _orderRepository = orderRepository;
        _currentAccountService = currentAccountService;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _mapper = mapper;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
    }

    public async Task<Result<Result>> Handle(OrderListInforNotificationQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var dictionaryAccountId = _orderRepository.GetDictionaryAccountIdRelated(request.Ids.ToList());
        var result = new List<Dictionary<object, object?>>();
        foreach (var accountDic in dictionaryAccountId)
        {
            var accounts = _accountRepository.GetAccountByIds(accountDic.Value.ToList());
            var response = _mapper.Map<List<AccountInforInChatRepsonse>>(accounts);
            result.Add(AccountInformationChatConverter.ConvertListToDictionaryFormat(response, accountDic.Key, _shopRepository));
        }

        return Result.Success(result);
    }

    private void Validate(OrderListInforNotificationQuery request)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : (account.RoleId == (int) Domain.Enums.Roles.ShopDelivery ? _shopDeliveryStaffRepository.GetById(account.Id).ShopId : 0);

        foreach (var id in request.Ids)
        {
            var order = _orderRepository
                .Get(o => o.Id == id && (o.ShopId == shopId || o.CustomerId == _currentPrincipalService.CurrentPrincipalId)).SingleOrDefault();
            if (order == default)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { id }, HttpStatusCode.NotFound);
        }
    }
}