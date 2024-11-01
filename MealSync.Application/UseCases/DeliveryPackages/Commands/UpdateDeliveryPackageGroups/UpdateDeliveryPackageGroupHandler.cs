using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageGroupDetailByTimeFrames;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public UpdateDeliveryPackageGroupHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IDeliveryPackageRepository deliveryPackageRepository, IOrderRepository orderRepository,
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ILogger<UpdateDeliveryPackageGroupHandler> logger, IDapperService dapperService)
    {
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _orderRepository = orderRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _logger = logger;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(UpdateDeliveryPackageGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Update section
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            // Update list
            var deliveryPackageUpdateIds = request.DeliveryPackages.Where(dp => dp.DeliveryPackageId.HasValue).Select(dp => dp.DeliveryPackageId.Value).ToList();
            var orderUpdateIds = request.DeliveryPackages.SelectMany(dp => dp.OrderIds).ToList();

            // OriginList
            var firstOrder = _orderRepository.GetById(request.DeliveryPackages.First().OrderIds.First());
            var deliveryPackages = _deliveryPackageRepository.GetPackagesByFrameAndDate(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.EndTime,
                _currentPrincipalService.CurrentPrincipalId.Value);
            var deliveryPackageOriginIds = deliveryPackages.Select(dp => dp.Id).ToList();
            var orderOriginIds = deliveryPackages.SelectMany(dp => dp.Orders.Select(o => o.Id)).ToList();

            // Delivery package
            var dpIdsUp = deliveryPackageUpdateIds.Intersect(deliveryPackageOriginIds).ToList();
            var dpIdsDel = deliveryPackageOriginIds.Except(deliveryPackageUpdateIds).ToList();

            // Order
            var oIdsUp = orderUpdateIds.Intersect(orderOriginIds).ToList();
            var oIdsDel = orderOriginIds.Except(orderUpdateIds).ToList();

            // Process update, add, delete delivery package
            await UpdateAndAddDeliveryPackageAsync(deliveryPackages, dpIdsUp, dpIdsDel, oIdsUp, oIdsDel, request).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(await GetUpdatedDeliveryPackageGroupAsync(firstOrder.IntendedReceiveDate, firstOrder.StartTime, firstOrder.EndTime).ConfigureAwait(false));
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success();
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

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderByDeliveryPackageIdAsync(long? packageId)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop,
            OrderForShopByStatusResponse> map = (parent, child1, child2, child3) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.Customer = child1;
                if (child2.DeliveryPackageId != 0 && (child2.Id != 0 || child2.IsShopOwnerShip))
                {
                    parent.ShopDeliveryStaff = child2;
                }

                parent.Foods.Add(child3);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child3);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        await _dapperService
            .SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop,
                OrderForShopByStatusResponse>(
                QueryName.GetListOrderByPackageId,
                map,
                new
                {
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                    DeliveryPackageId = packageId,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderUnAssignByTimeFrameAsync(DateTime intendedReceiveDate, int startTime, int endTime)
    {
        var orderUniq = new Dictionary<long, OrderForShopByStatusResponse>();
        Func<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop,
            OrderForShopByStatusResponse> map = (parent, child1, child2, child3) =>
        {
            if (!orderUniq.TryGetValue(parent.Id, out var order))
            {
                parent.Customer = child1;
                if (child2.DeliveryPackageId != 0 && (child2.Id != 0 || child2.IsShopOwnerShip))
                {
                    parent.ShopDeliveryStaff = child2;
                }

                parent.Foods.Add(child3);
                orderUniq.Add(parent.Id, parent);
            }
            else
            {
                order.Foods.Add(child3);
                orderUniq.Remove(order.Id);
                orderUniq.Add(order.Id, order);
            }

            return parent;
        };

        await _dapperService
            .SelectAsync<OrderForShopByStatusResponse, OrderForShopByStatusResponse.CustomerInforInOrderForShop, OrderForShopByStatusResponse.ShopDeliveryStaffInOrderForShop, OrderForShopByStatusResponse.FoodInOrderForShop,
                OrderForShopByStatusResponse>(
                QueryName.GetListOrderUnAssignInTimeFrame,
                map,
                new
                {
                    IntendedReceiveDate = intendedReceiveDate.ToString("yyyy-MM-dd"),
                    StartTime = startTime,
                    EndTime = endTime,
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }

    private async Task UpdateAndAddDeliveryPackageAsync(List<DeliveryPackage> dpOrigins, List<long> dpIdsUp, List<long> dpIdsDel, List<long> oIdsUp, List<long> oIdsDel, UpdateDeliveryPackageGroupCommand request)
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
                var deliverPackage = request.DeliveryPackages.Single(dp => dp.DeliveryPackageId.HasValue && dp.DeliveryPackageId.Value == dpO.Id);
                dpO.ShopDeliveryStaffId = deliverPackage.ShopDeliveryStaffId;
                dpO.ShopId = deliverPackage.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null;


                if (deliverPackage.ShopDeliveryStaffId != null)
                {
                    var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(deliverPackage.ShopDeliveryStaffId.Value);
                    shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                    _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
                }

                // Update order exist
                var oIdsUpOfDp = request.DeliveryPackages.Where(dp => dp.DeliveryPackageId.Value == dpO.Id).First();
                var ordersUpdate = _orderRepository.GetByIds(oIdsUpOfDp.OrderIds.ToList());
                // ordersUpdate.ForEach(o =>
                // {
                //     o.DeliveryPackageId = dpO.Id;
                // });
                // ordersUpdateSaveChange.AddRange(ordersUpdate);
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
        foreach (var dpRequest in request.DeliveryPackages.Where(dp => !dp.DeliveryPackageId.HasValue))
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = dpRequest.ShopDeliveryStaffId,
                ShopId = dpRequest.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = firstOrder.IntendedReceiveDate,
                StartTime = firstOrder.StartTime,
                EndTime = firstOrder.EndTime,
                Status = DeliveryPackageStatus.Created,
            };

            var ordersUpdateToNewDelvieryPackage = _orderRepository.GetByIds(dpRequest.OrderIds.ToList());
            dp.Orders = ordersUpdateToNewDelvieryPackage;

            deliveryPackageAddNewSaveChange.Add(dp);

            if (dpRequest.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(dpRequest.ShopDeliveryStaffId.Value);
                shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }
        }

        await _deliveryPackageRepository.AddRangeAsync(deliveryPackageAddNewSaveChange).ConfigureAwait(false);
        _deliveryPackageRepository.UpdateRange(deliveryPackageUpdateSaveChange);
        _orderRepository.UpdateRange(ordersUpdateSaveChange);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        // Delete physic delivery package
        _deliveryPackageRepository.RemoveRange(deliveryPackageRemoveSaveChange);
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
            if (deliveryPackage.DeliveryPackageId.HasValue && _deliveryPackageRepository.Get(dp => dp.Id == deliveryPackage.DeliveryPackageId.Value && (
                    dp.ShopId != null && dp.ShopId == _currentPrincipalService.CurrentPrincipalId.Value ||
                    dp.ShopDeliveryStaffId != null && dp.ShopDeliveryStaff.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)).SingleOrDefault() == default)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_NOT_FOUND.GetDescription(), new object[] { deliveryPackage.DeliveryPackageId.Value }, HttpStatusCode.NotFound);

            if (deliveryPackage.ShopDeliveryStaffId.HasValue && _shopDeliveryStaffRepository.Get(sds => sds.Id == deliveryPackage.ShopDeliveryStaffId && sds.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault() == default)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_NOT_BELONG_TO_SHOP.GetDescription(), new object[] { deliveryPackage.ShopDeliveryStaffId }, HttpStatusCode.NotFound);

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
}