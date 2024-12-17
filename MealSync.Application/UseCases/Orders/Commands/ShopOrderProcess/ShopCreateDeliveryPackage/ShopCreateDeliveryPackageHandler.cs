using System.Net;
using System.Runtime.Intrinsics.Arm;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
    private readonly IChatService _chatService;

    public ShopCreateDeliveryPackageHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IDeliveryPackageRepository deliveryPackageRepository, ILogger<ShopCreateDeliveryPackageHandler> logger,
        INotificationFactory notificationFactory, INotifierService notifierService, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IShopRepository shopRepository,
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository, IDapperService dapperService, IMapper mapper, IChatService chatService)
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
        _chatService = chatService;
    }

    public async Task<Result<Result>> Handle(ShopCreateDeliveryPackageCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        var firstOrderId = request.DeliveryPackages.First().OrderIds.First();
        var order = _orderRepository.GetById(firstOrderId);
        if (!request.IsConfirm.Value)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var intendedReceiveDateTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS);
            if (now < endTime)
            {
                var diffDate = endTime.AddHours(OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS) - now;
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                        new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), $"{diffDate.Hours}:{diffDate.Minutes}" }),
                });
            }
        }

        var deliveryPackages = new List<DeliveryPackage>();
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var deliveryPackageRequest in request.DeliveryPackages)
            {
                var deliveryPackage = await CreateOrderSaveDeliveryPackageAsync(deliveryPackageRequest).ConfigureAwait(false);
                deliveryPackages.Add(deliveryPackage);
            }

            await _deliveryPackageRepository.AddRangeAsync(deliveryPackages).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var listNoti = new List<Notification>();
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);

        // Noti to shop staff about delivery package
        foreach (var dp in deliveryPackages)
        {
            if (dp.ShopDeliveryStaffId.HasValue)
            {
                var accShip = _accountRepository.GetById(dp.ShopDeliveryStaffId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                listNoti.Add(notiShopStaff);
            }
        }
        _notifierService.NotifyRangeAsync(listNoti);
        var deliveryPackageGroup = await GetListDeliveryPackageGroupDetailAsync(order.IntendedReceiveDate, order.StartTime, order.EndTime).ConfigureAwait(false);

        var unAssignOrder = await GetListOrderUnAssignByTimeFrameAsync(order.IntendedReceiveDate, order.StartTime, order.EndTime).ConfigureAwait(false);
        return Result.Success(new
        {
            DeliverPackageGroup = deliveryPackageGroup,
            UnassignOrder = unAssignOrder,
        });
    }

    private async Task<DeliveryPackage> CreateOrderSaveDeliveryPackageAsync(DeliveryPackageRequest request)
    {
        var orders = _orderRepository.Get(o => request.OrderIds.Contains(o.Id)).ToList();

        // Update orders history
        // Save an history assign
        var shipperIdAssign = request.ShopDeliveryStaffId.HasValue ? request.ShopDeliveryStaffId.Value : _currentPrincipalService.CurrentPrincipalId.Value;
        foreach (var order in orders)
        {
            if (order.HistoryAssignJson != null)
            {
                var history = JsonConvert.DeserializeObject<List<HistoryAssign>>(order.HistoryAssignJson);

                // Send noti to add shipper
                if (shipperIdAssign != order.ShopId && history.All(h => h.Id != shipperIdAssign))
                {
                    var shipperAccount = _accountRepository.GetById(shipperIdAssign);
                    var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, shipperAccount);

                    _chatService.OpenOrCloseRoom(new AddChat()
                    {
                        IsOpen = true,
                        RoomId = order.Id,
                        UserId = shipperIdAssign,
                        Notification = notificationJoinRoom,
                    });
                }

                history.Add(new HistoryAssign()
                {
                    Id = shipperIdAssign,
                    AssignDate = DateTimeOffset.UtcNow,
                });
                order.HistoryAssignJson = JsonConvert.SerializeObject(history);
            }
            else
            {
                var history = new List<HistoryAssign>();
                history.Add(new HistoryAssign()
                {
                    Id = shipperIdAssign,
                    AssignDate = DateTimeOffset.UtcNow,
                });
                order.HistoryAssignJson = JsonConvert.SerializeObject(history);

                // Send noti to add shipper
                if (shipperIdAssign != order.ShopId)
                {
                    var shipperAccount = _accountRepository.GetById(shipperIdAssign);
                    var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, shipperAccount);

                    _chatService.OpenOrCloseRoom(new AddChat()
                    {
                        IsOpen = true,
                        RoomId = order.Id,
                        UserId = shipperIdAssign,
                        Notification = notificationJoinRoom,
                    });
                }
            }
        }

        // Need create new delivery package
        var dp = new DeliveryPackage()
        {
            ShopDeliveryStaffId = request.ShopDeliveryStaffId,
            ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
            DeliveryDate = TimeFrameUtils.GetCurrentDateInUTC7().Date,
            StartTime = orders.First().StartTime,
            EndTime = orders.First().EndTime,
            Status = DeliveryPackageStatus.InProcess,

        };
        dp.Orders = orders;

        if (request.ShopDeliveryStaffId != null)
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
            // shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
            _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
        }

        return dp;
    }

    private void Validate(ShopCreateDeliveryPackageCommand request)
    {
        var listOrder = new List<Order>();
        foreach (var deliveryPackage in request.DeliveryPackages)
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.Get(sds => sds.Id == deliveryPackage.ShopDeliveryStaffId && sds.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                .Include(sds => sds.Account).SingleOrDefault();
            if (deliveryPackage.ShopDeliveryStaffId.HasValue && shopDeliveryStaff == default)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_NOT_BELONG_TO_SHOP.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId }, HttpStatusCode.NotFound);

            if (deliveryPackage.ShopDeliveryStaffId.HasValue && shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Offline)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_OFFLINE_STATUS.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });

            if (deliveryPackage.ShopDeliveryStaffId.HasValue && shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status == AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });

            if (deliveryPackage.ShopDeliveryStaffId.HasValue && shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status != AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_ACTIVE.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });

            foreach (var orderId in deliveryPackage.OrderIds)
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

                var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
                var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
                if (currentDateTime.DateTime > startEndTime.EndTime)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { order.Id });

                if (order.DeliveryPackageId != null)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_IN_OTHER_PACKAGE.GetDescription(), new object[] { orderId });

                listOrder.Add(order);
            }

            var firstOrderInPackage = listOrder.FirstOrDefault() ?? new Order();
            var shipperId = deliveryPackage.ShopDeliveryStaffId.HasValue ? deliveryPackage.ShopDeliveryStaffId.Value : _currentPrincipalService.CurrentPrincipalId.Value;
            var deliveryPackageCheck = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(!deliveryPackage.ShopDeliveryStaffId.HasValue,
                shipperId, firstOrderInPackage.StartTime, firstOrderInPackage.EndTime);
            if (deliveryPackageCheck != null)
            {
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_ALREADY_HAVE_OTHER_PACKAGE.GetDescription(), new object[] { shipperId });
            }

            // Check is exist any delivery package for this frame
            var deliveryPackageCheckExist = _deliveryPackageRepository.GetPackagesByFrameAndDate(TimeFrameUtils.GetCurrentDateInUTC7().Date, firstOrderInPackage.StartTime, firstOrderInPackage.EndTime, _currentPrincipalService.CurrentPrincipalId.Value);
            if (deliveryPackageCheckExist != null && deliveryPackageCheckExist.Count > 0)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_TIME_FRAME_CREATED.GetDescription(), new object[] { TimeFrameUtils.GetTimeFrameString(firstOrderInPackage.StartTime, firstOrderInPackage.EndTime) });
        }

        var differentOrders = GetDifferentTimeOrders(listOrder);
        if (differentOrders != null)
        {
            var listOrderIds = differentOrders.Select(x => string.Concat(IdPatternConstant.PREFIX_ID, x.Id)).ToList();
            var joinListOrderIds = string.Join(',', listOrderIds);
            var firstOrderCompare = listOrder.FirstOrDefault() ?? new Order();
            throw new InvalidBusinessException(MessageCode.E_ORDER_IN_DIFFERENT_FRAME.GetDescription(), new object[]
            {
                joinListOrderIds, TimeFrameUtils.GetTimeFrameString(firstOrderCompare.StartTime, firstOrderCompare.EndTime),
            });
        }
    }

    private List<Order> GetDifferentTimeOrders(List<Order> orders)
    {
        // Check if the list is empty or only contains one order (no comparison needed)
        if (orders == null || orders.Count <= 1)
            return null;

        // Get the reference times from the first order
        var firstOrder = orders.First();
        var startTime = firstOrder.StartTime;
        var endTime = firstOrder.EndTime;

        // Find orders with different StartTime or EndTime
        var differentOrders = orders
            .Where(o => o.StartTime != startTime || o.EndTime != endTime)
            .ToList();

        // Return the list of different orders, or null if there are none
        return differentOrders.Any() ? differentOrders : null;
    }

    private async Task<List<DeliveryPackageGroupDetailResponse>> GetListDeliveryPackageGroupDetailAsync(DateTime intendedReceiveDate, int startTime, int endTime)
    {
        var dicDormitoryUniq = new Dictionary<long, DeliveryPackageGroupDetailResponse>();
        Func<DeliveryPackageGroupDetailResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse> map =
            (parent, child1, child2, child3) =>
            {
                if (!dicDormitoryUniq.TryGetValue(parent.DeliveryPackageId, out var deliveryPackage))
                {
                    parent.ShopDeliveryStaff = child1;

                    child2.ShopDeliveryStaff = child3;
                    parent.Dormitories.Add(child2);
                    dicDormitoryUniq.Add(parent.DeliveryPackageId, parent);
                }
                else
                {
                    child2.ShopDeliveryStaff = child3;
                    deliveryPackage.Dormitories.Add(child2);
                    dicDormitoryUniq.Remove(deliveryPackage.DeliveryPackageId);
                    dicDormitoryUniq.Add(deliveryPackage.DeliveryPackageId, deliveryPackage);
                }

                return parent;
            };

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse>(
            QueryName.GetListDeliveryDetailStasticByTimeFrame,
            map,
            new
            {
                IntendedReceiveDate = intendedReceiveDate.ToString("yyyy-MM-dd"),
                StartTime = startTime,
                EndTime = endTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            },
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackages = dicDormitoryUniq.Values.ToList();
        foreach (var deliveryPackage in deliveryPackages)
        {
            deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId).ConfigureAwait(false);
        }

        return deliveryPackages;
    }

    private async Task<List<OrderDetailForShopResponse>> GetListOrderByDeliveryPackageIdAsync(long? packageId)
    {
        var uniqOrder = new Dictionary<long, OrderDetailForShopResponse>();
        Func<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail, OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail,
            OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse> map =
            (parent, child1, child2, child3, child4) =>
            {
                if (!uniqOrder.TryGetValue(parent.Id, out var order))
                {
                    parent.Customer = child1;
                    if (child2.Id != 0)
                    {
                        parent.Promotion = child2;
                    }

                    if (child3.DeliveryPackageId != 0 && (child3.Id != 0 || child3.IsShopOwnerShip))
                    {
                        parent.ShopDeliveryStaff = child3;
                    }

                    parent.OrderDetails.Add(child4);
                    uniqOrder.Add(parent.Id, parent);
                }
                else
                {
                    order.OrderDetails.Add(child4);
                    uniqOrder.Remove(order.Id);
                    uniqOrder.Add(order.Id, order);
                }

                return parent;
            };

        await _dapperService
            .SelectAsync<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail,
                OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail, OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse>(
                QueryName.GetListOrderByPackageId,
                map,
                new
                {
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                    DeliveryPackageId = packageId,
                },
                "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return uniqOrder.Values.ToList();
    }

    private async Task<List<OrderDetailForShopResponse>> GetListOrderUnAssignByTimeFrameAsync(DateTime intendedReceiveDate, int startTime, int endTime)
    {
        var uniqOrder = new Dictionary<long, OrderDetailForShopResponse>();
        Func<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail, OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail,
            OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse> map =
            (parent, child1, child2, child3, child4) =>
            {
                if (!uniqOrder.TryGetValue(parent.Id, out var order))
                {
                    parent.Customer = child1;
                    if (child2.Id != 0)
                    {
                        parent.Promotion = child2;
                    }

                    if (child3.DeliveryPackageId != 0 && (child3.Id != 0 || child3.IsShopOwnerShip))
                    {
                        parent.ShopDeliveryStaff = child3;
                    }

                    parent.OrderDetails.Add(child4);
                    uniqOrder.Add(parent.Id, parent);
                }
                else
                {
                    order.OrderDetails.Add(child4);
                    uniqOrder.Remove(order.Id);
                    uniqOrder.Add(order.Id, order);
                }

                return parent;
            };

        await _dapperService
            .SelectAsync<OrderDetailForShopResponse, OrderDetailForShopResponse.CustomerInforInShoprderDetailForShop, OrderDetailForShopResponse.PromotionInShopOrderDetail,
                OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail, OrderDetailForShopResponse.FoodInShopOrderDetail, OrderDetailForShopResponse>(
                QueryName.GetListOrderUnAssignInTimeFrame,
                map,
                new
                {
                    IntendedReceiveDate = intendedReceiveDate.ToString("yyyy-MM-dd"),
                    StartTime = startTime,
                    EndTime = endTime,
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                },
                "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return uniqOrder.Values.ToList();
    }
}