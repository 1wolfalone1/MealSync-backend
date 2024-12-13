using System.Net;
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

namespace MealSync.Application.UseCases.DeliveryPackages.Commands.UpdateDeliveryPackageGroups;

public class UpdateDeliveryPackageGroupHandler : ICommandHandler<UpdateDeliveryPackageGroupCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ILogger<UpdateDeliveryPackageGroupHandler> _logger;
    private readonly IDapperService _dapperService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IChatService _chatService;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;
    private readonly INotifierService _notifierService;

    public UpdateDeliveryPackageGroupHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IDeliveryPackageRepository deliveryPackageRepository, IOrderRepository orderRepository,
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ILogger<UpdateDeliveryPackageGroupHandler> logger, IDapperService dapperService, INotificationFactory notificationFactory, IChatService chatService, IAccountRepository accountRepository, IShopRepository shopRepository, INotifierService notifierService)
    {
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _orderRepository = orderRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _logger = logger;
        _dapperService = dapperService;
        _notificationFactory = notificationFactory;
        _chatService = chatService;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
        _notifierService = notifierService;
    }

    public async Task<Result<Result>> Handle(UpdateDeliveryPackageGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Update section
        var firstOrder = _orderRepository.GetById(request.DeliveryPackages.First().OrderIds.First());
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);


            // Update list
            var staffIds = request.DeliveryPackages.Select(x =>
            {
                if (x.ShopDeliveryStaffId.HasValue)
                    return x.ShopDeliveryStaffId.Value;

                return _currentPrincipalService.CurrentPrincipalId.Value;
            }).ToList();
            var deliveryPackageRequestUpdate = _deliveryPackageRepository.GetAllRequestUpdate(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.StartTime, staffIds);
            var deliveryPackageUpdateIds = deliveryPackageRequestUpdate.Select(dp => dp.Id).ToList();
            var orderUpdateIds = request.DeliveryPackages.SelectMany(dp => dp.OrderIds).ToList();
            var staffIdsInPackageRequestLocatedInDb = deliveryPackageRequestUpdate.Select(dp =>
            {
                if (dp.ShopDeliveryStaffId.HasValue)
                    return dp.ShopDeliveryStaffId.Value;

                return dp.ShopId.Value;
            }).ToList();

            // OriginList
            var deliveryPackages = _deliveryPackageRepository.GetPackagesByFrameAndDate(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.EndTime,
                _currentPrincipalService.CurrentPrincipalId.Value);
            var deliveryPackageOriginIds = deliveryPackages.Select(dp => dp.Id).ToList();
            var orderOriginIds = deliveryPackages.SelectMany(dp => dp.Orders.Select(o => o.Id)).ToList();

            // Delivery package
            var dpIdsUp = deliveryPackageUpdateIds.Intersect(deliveryPackageOriginIds).ToList();
            var dpIdsDel = deliveryPackageOriginIds.Except(deliveryPackageUpdateIds).ToList();
            var dpStaffIdsNew = staffIds.Except(staffIdsInPackageRequestLocatedInDb).ToList();

            // Order
            var oIdsUp = orderUpdateIds.Intersect(orderOriginIds).ToList();
            var oIdsDel = orderOriginIds.Except(orderUpdateIds).ToList();

            // Process update, add, delete delivery package
            await UpdateAndAddDeliveryPackageAsync(deliveryPackages, dpIdsUp, dpIdsDel, dpStaffIdsNew, oIdsUp, oIdsDel, request).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(await GetUpdatedDeliveryPackageGroupAsync(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.EndTime).ConfigureAwait(false));
    }

    private async Task<DeliveryPackageIntervalResponse> GetUpdatedDeliveryPackageGroupAsync(DateTime intendedReceiveDate, int startTime, int endTime)
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

        var unassignOrder = await GetListOrderUnAssignByTimeFrameAsync(intendedReceiveDate, startTime, endTime).ConfigureAwait(false);

        return new DeliveryPackageIntervalResponse()
        {
            IntendedReceiveDate = intendedReceiveDate,
            StartTime = startTime,
            EndTime = endTime,
            DeliveryPackageGroups = deliveryPackages,
            UnassignOrders = unassignOrder,
        };
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

    private async Task UpdateAndAddDeliveryPackageAsync(List<DeliveryPackage> dpOrigins, List<long> dpIdsUp, List<long> dpIdsDel, List<long> dpStaffIdsAddNew, List<long> oIdsUp, List<long> oIdsDel, UpdateDeliveryPackageGroupCommand request)
    {
        var ordersUpdateSaveChange = new List<Order>();
        var deliveryPackageUpdateSaveChange = new List<DeliveryPackage>();
        var deliveryPackageRemoveSaveChange = new List<DeliveryPackage>();
        var deliveryPackageAddNewSaveChange = new List<DeliveryPackage>();
        foreach (var dpO in dpOrigins)
        {
            // Update
            if (dpIdsUp.Contains(dpO.Id))
            {
                // Update order exist
                var oIdsUpOfDp = request.DeliveryPackages.Where(dp => dpO.ShopId.HasValue && !dp.ShopDeliveryStaffId.HasValue ||
                                                                      !dpO.ShopId.HasValue && dpO.ShopDeliveryStaffId.Value == dp.ShopDeliveryStaffId.Value).First();
                var ordersUpdate = _orderRepository.GetByIds(oIdsUpOfDp.OrderIds.ToList());

                // Add history assign to order
                var shipperIdAssign = dpO.ShopDeliveryStaffId.HasValue ? dpO.ShopDeliveryStaffId.Value : _currentPrincipalService.CurrentPrincipalId.Value;
                ordersUpdate = UpdateOrderHistoryAssign(ordersUpdate, shipperIdAssign);
                dpO.Orders = ordersUpdate;
                deliveryPackageUpdateSaveChange.Add(dpO);
            }

            // Delete
            else if (dpIdsDel.Contains(dpO.Id))
            {
                // Delete need do when order have been change all
                deliveryPackageRemoveSaveChange.Add(dpO);

                var oIdsDelOfDp = dpO.Orders.Where(o => oIdsDel.Contains(o.Id)).Select(o => o.Id).ToList();
                var ordersDel = _orderRepository.GetByIds(oIdsDelOfDp);
                ordersDel.ForEach(o =>
                {
                    o.DeliveryPackageId = null;
                });
                ordersUpdateSaveChange.AddRange(ordersDel);
            }
        }

        // Add new
        var firstOrder = _orderRepository.GetById(request.DeliveryPackages.First().OrderIds.First());
        foreach (var dpRequest in request.DeliveryPackages.Where(dp => dp.ShopDeliveryStaffId.HasValue && dpStaffIdsAddNew.Contains(dp.ShopDeliveryStaffId.Value) ||
                                                                       !dp.ShopDeliveryStaffId.HasValue && dpStaffIdsAddNew.Contains(_currentPrincipalService.CurrentPrincipalId.Value)))
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = dpRequest.ShopDeliveryStaffId,
                ShopId = dpRequest.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = firstOrder.IntendedReceiveDate,
                StartTime = firstOrder.StartTime,
                EndTime = firstOrder.EndTime,
                Status = DeliveryPackageStatus.InProcess,
            };

            var ordersUpdateToNewDelvieryPackage = _orderRepository.GetByIds(dpRequest.OrderIds.ToList());

            // Add history assign to order
            var shipperIdAssign = dpRequest.ShopDeliveryStaffId.HasValue ? dpRequest.ShopDeliveryStaffId.Value : _currentPrincipalService.CurrentPrincipalId.Value;
            ordersUpdateToNewDelvieryPackage = UpdateOrderHistoryAssign(ordersUpdateToNewDelvieryPackage, shipperIdAssign);
            dp.Orders = ordersUpdateToNewDelvieryPackage;

            deliveryPackageAddNewSaveChange.Add(dp);

            if (dpRequest.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(dpRequest.ShopDeliveryStaffId.Value);
                // shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }
        }

        await _deliveryPackageRepository.AddRangeAsync(deliveryPackageAddNewSaveChange).ConfigureAwait(false);
        _deliveryPackageRepository.UpdateRange(deliveryPackageUpdateSaveChange);
        _orderRepository.UpdateRange(ordersUpdateSaveChange);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        // Delete physic delivery package
        _deliveryPackageRepository.RemoveRange(deliveryPackageRemoveSaveChange);

        // Noti to shop staff about delivery package
        var deliveryPackageHaveChange = deliveryPackageAddNewSaveChange;
        deliveryPackageHaveChange.AddRange(deliveryPackageUpdateSaveChange);
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        var listNoti = new List<Notification>();
        foreach (var dp in deliveryPackageHaveChange)
        {
            if (dp.ShopDeliveryStaffId.HasValue)
            {
                var accShip = _accountRepository.GetById(dp.ShopDeliveryStaffId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                listNoti.Add(notiShopStaff);
            }
        }

        _notifierService.NotifyRangeAsync(listNoti);
    }

    private void Validate(UpdateDeliveryPackageGroupCommand request)
    {
        // Valid is this time frame really created
        var firstOrder = _orderRepository.GetById(request.DeliveryPackages.First().OrderIds.First());
        var deliveryPackages = _deliveryPackageRepository.GetPackagesByFrameAndDate(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.EndTime,
            _currentPrincipalService.CurrentPrincipalId.Value);
        if (deliveryPackages == null || deliveryPackages.Count == 0)
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_GROUP_NOT_FOUND_ANY.GetDescription(), HttpStatusCode.NotFound);

        var listOrder = new List<Order>();
        foreach (var deliveryPackage in request.DeliveryPackages)
        {
            if (deliveryPackage.ShopDeliveryStaffId.HasValue)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.Get(sds => sds.Id == deliveryPackage.ShopDeliveryStaffId && sds.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                    .Include(sds => sds.Account).SingleOrDefault();
                if (shopDeliveryStaff == null || shopDeliveryStaff.ShopId != _currentPrincipalService.CurrentPrincipalId)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_ASSIGN_NOT_FOUND_SHOP_STAFF.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId.Value });

                if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Offline)
                    throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_OFFLINE_STATUS.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });

                if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status == AccountStatus.Deleted)
                    throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });

                if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status != AccountStatus.Deleted)
                    throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_ACTIVE.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId });
            }

            foreach (var orderId in deliveryPackage.OrderIds)
            {
                var order = _orderRepository
                    .Get(o => o.Id == orderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                    .Include(o => o.DeliveryPackage).SingleOrDefault();

                if (order == default)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { orderId }, HttpStatusCode.NotFound);

                if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });

                if (OrderConstant.LIST_ORDER_STATUS_FIX_ORDER_ASSIGN_PROCESS.Contains(order.Status) && order.DeliveryPackage != null
                                                                                                    && (order.DeliveryPackage.ShopDeliveryStaffId.HasValue
                                                                                                        && order.DeliveryPackage.ShopDeliveryStaffId != deliveryPackage.ShopDeliveryStaffId.Value
                                                                                                        || !order.DeliveryPackage.ShopDeliveryStaffId.HasValue
                                                                                                        && order.DeliveryPackage.ShopId != _currentPrincipalService.CurrentPrincipalId.Value))
                    throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_DATE_UPDATE_NOT_NEW.GetDescription());

                var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
                var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
                if (currentDateTime.DateTime > startEndTime.EndTime)
                    throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { orderId });

                listOrder.Add(order);
            }

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

    private List<Order> UpdateOrderHistoryAssign(List<Order> orders, long shipperIdAssign)
    {
        foreach (var order in orders)
        {
            if (order.HistoryAssignJson != null)
            {
                var history = JsonConvert.DeserializeObject<List<HistoryAssign>>(order.HistoryAssignJson);
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
            }
        }

        return orders;
    }
}