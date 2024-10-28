using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopCreateDeliveryPackage;

public class ShopCreateDeliveryPackageHandler : ICommandHandler<ShopCreateDeliveryPackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ILogger<ShopCreateDeliveryPackageHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IDapperService _dapperService;
    private readonly IMapper _mapper;

    public ShopCreateDeliveryPackageHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IDeliveryPackageRepository deliveryPackageRepository, ILogger<ShopCreateDeliveryPackageHandler> logger,
        INotificationFactory notificationFactory, INotifierService notifierService, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IShopRepository shopRepository, IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository, IDapperService dapperService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _dapperService = dapperService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopCreateDeliveryPackageCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var orders = _orderRepository.Get(o => request.OrderIds.Contains(o.Id)).ToList();

            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = request.ShopDeliveryStaffId,
                ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = TimeFrameUtils.GetCurrentDateInUTC7().Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = DeliveryPackageStatus.Created,

            };
            dp.Orders = orders;
            await _deliveryPackageRepository.AddAsync(dp).ConfigureAwait(false);

            if (request.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
                shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Noti to customer
            var listNoti = new List<Notification>();
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            foreach (var order in orders)
            {
                var notiCus = _notificationFactory.CreateOrderCustomerDeliveringNotification(order, shop);
                listNoti.Add(notiCus);
            }

            // Noti to shop staff about delivery package
            if (request.ShopDeliveryStaffId != null)
            {
                var accShip = _accountRepository.GetById(request.ShopDeliveryStaffId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                listNoti.Add(notiShopStaff);
            }

            _notifierService.NotifyRangeAsync(listNoti);

            var response = _mapper.Map<DeliveryPackageResponse>(dp);
            response.Orders = await GetListOrderByDeliveryPackageIdAsync(dp.Id).ConfigureAwait(false);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopCreateDeliveryPackageCommand request)
    {
        foreach (var orderId in request.OrderIds)
        {
            var order = _orderRepository
                .Get(o => o.Id == orderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                .Include(o => o.DeliveryPackage).SingleOrDefault();

            if (order == default)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { orderId }, HttpStatusCode.NotFound);

            if (order.Status != OrderStatus.Preparing)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { orderId });

            if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });

            if (order.DeliveryPackageId != null)
                throw new InvalidBusinessException(MessageCode.E_ORDER_IN_OTHER_PACKAGE.GetDescription(), new object[] { orderId });

            if (order.StartTime != request.StartTime || order.EndTime != request.EndTime)
                throw new InvalidBusinessException(MessageCode.E_ORDER_IN_DIFFERENT_FRAME.GetDescription(), new object[]
                {
                    orderId, TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), TimeFrameUtils.GetTimeFrameString(request.StartTime, request.EndTime),
                });
        }
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderByDeliveryPackageIdAsync(long packageId)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse> map = (parent, child1, child2) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.Customer = child1;
                parent.Foods.Add(child2);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child2);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        await _dapperService.SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop, OrderForShopByStatusResponse>(
            QueryName.GetListOrderByPackageId,
            map,
            new
            {
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                DeliveryPackageId = packageId,
            },
            "CustomerSection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }
}