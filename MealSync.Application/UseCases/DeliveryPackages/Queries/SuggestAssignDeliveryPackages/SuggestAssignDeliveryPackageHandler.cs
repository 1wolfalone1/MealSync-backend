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
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignDeliveryPackages;

public class SuggestAssignDeliveryPackageHandler : IQueryHandler<SuggestAssignDeliveryPackageQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public SuggestAssignDeliveryPackageHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(SuggestAssignDeliveryPackageQuery request, CancellationToken cancellationToken)
    {
        // Valiate
        Validate(request);

        var unAssignOrders = await GetListOrderUnAssignByTimeFrameAsync(request).ConfigureAwait(false);
        var shopStaffs = await GetListShopDeliveryStaffAsync(request).ConfigureAwait(false);

        // Check if shop not ship remove shop
        shopStaffs = shopStaffs.Where(sf => request.ShipperIds.Contains(sf.ShopDeliveryStaff.Id)).ToList();

        var assignOrder = AssignOrder(shopStaffs, unAssignOrders);

        return Result.Success(new
        {
            IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DeliveryPackageGroups = assignOrder.AssignedStaff,
            UnassignOrders = assignOrder.UnAssignOrder,
        });
    }

    private void Validate(SuggestAssignDeliveryPackageQuery request)
    {
        var deliveryPackage = _deliveryPackageRepository.GetPackagesByFrameAndDate(TimeFrameUtils.GetCurrentDateInUTC7().Date, request.StartTime, request.EndTime, _currentPrincipalService.CurrentPrincipalId.Value);
        if (deliveryPackage != null && deliveryPackage.Count > 0)
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_TIME_FRAME_CREATED.GetDescription(), new object[] { TimeFrameUtils.GetTimeFrameString(request.StartTime, request.EndTime) });

        foreach (var id in request.ShipperIds.Where(s => s != 0).ToList())
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.Get(sds => sds.Id == id && sds.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                .Include(sds => sds.Account).SingleOrDefault();
            if (shopDeliveryStaff == default)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_NOT_BELONG_TO_SHOP.GetDescription(), new object[] { id }, HttpStatusCode.NotFound);

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Offline)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_OFFLINE_STATUS.GetDescription(), new object[] { id });

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status == AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { id });

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status != AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_ACTIVE.GetDescription(), new object[] { id });
        }
    }

    private (List<DeliveryPackageForAssignResponse> AssignedStaff, List<OrderDetailForShopResponse> UnAssignOrder) AssignOrder(List<DeliveryPackageForAssignResponse> listStaff, List<OrderDetailForShopResponse> orders)
    {
        var ordersByDormitory = orders.GroupBy(o => o.DormitoryId)
            .ToDictionary(g => g.Key, g => (g.Count(), g.ToList()));

        // Calculate the ideal number of orders each staff should handle
        var equallyOrderForEachStaff = Math.Round(orders.Count / listStaff.Count * 1.0, MidpointRounding.ToEven);

        var staffIndex = 0;
        List<(long DormitoryId, long NumberOfOrder)> dormitoryIdsSatisfyCondition = new();
        foreach (var orderDormitory in ordersByDormitory)
        {
            if (orderDormitory.Value.Item1 <= equallyOrderForEachStaff)
            {
                dormitoryIdsSatisfyCondition.Add((orderDormitory.Key, orderDormitory.Value.Item1));
            }
        }

        for (var i = 0; i < listStaff.Count(); i++)
        {
            // Assign for staff until not left
            if (dormitoryIdsSatisfyCondition.Count > 0)
            {
                // Get out list order can add full for a staff
                var dormitoryIdContainsNearestEquallyOrder = dormitoryIdsSatisfyCondition.OrderByDescending(x => x.NumberOfOrder).First();
                dormitoryIdsSatisfyCondition.Remove(dormitoryIdContainsNearestEquallyOrder);

                var listOrderOfDormitoryCanAssignAllToStaff = ordersByDormitory[dormitoryIdContainsNearestEquallyOrder.DormitoryId].Item2;
                ordersByDormitory.Remove(dormitoryIdContainsNearestEquallyOrder.DormitoryId);
                listStaff[i] = AssignOrderToStaff(listStaff[i], listOrderOfDormitoryCanAssignAllToStaff);
            }
        }

        // Assign left orders to shop delivery staff base on formular
        var listOrderLeft = ordersByDormitory.Values.SelectMany(x => x.Item2).ToList();
        if (listOrderLeft != null && listOrderLeft.Count() > 0)
        {
            var result = AssignOrderByFormular(listStaff, listOrderLeft);
            return (result.AssignedStaff, result.UnAssignOrder);
        }

        return (listStaff, new List<OrderDetailForShopResponse>());
    }

    private (List<DeliveryPackageForAssignResponse> AssignedStaff, List<OrderDetailForShopResponse> UnAssignOrder) AssignOrderByFormular(List<DeliveryPackageForAssignResponse> listStaff,
        List<OrderDetailForShopResponse> orders)
    {
        List<OrderDetailForShopResponse> unAssignOrder = new();
        Dictionary<long, DeliveryPackageForAssignResponse> assignedStaffs = new();
        foreach (var order in orders)
        {
            DeliveryPackageForAssignResponse bestStaff = null;
            var bestScore = double.MaxValue;

            foreach (var staff in listStaff)
            {
                // Calculate workload ratio
                var workloadRatio = staff.ShopDeliveryStaff.CurrentTaskLoad / DevidedOrderConstant.StaffMaxCapacity;

                /* If order need to delivery to different dormitory when compare to current dormitory in staff
                    need to check other staff have that dormitory is more than 50% totalOrder. If no let them take
                 */
                var salt = 0;
                if (staff.Dormitories.All(d => d.Id != order.DormitoryId) && staff.Total > 0)
                {
                    var staffWithMinTotalOrder = listStaff.Where(s => s.Dormitories.Select(d => d.Id).Contains(order.DormitoryId)).OrderBy(d => d.Total).FirstOrDefault();
                    if (staffWithMinTotalOrder == default)
                    {
                        salt = int.MaxValue;
                    }
                    else
                    {
                        var percentTaskCompare = staffWithMinTotalOrder.Total / staff.Total;
                        if (percentTaskCompare < DevidedOrderConstant.PercentOverTaskCanAccept)
                        {
                            salt = int.MaxValue;
                        }
                    }
                }

                // Calculate the score (lower score is better)
                var score = DevidedOrderConstant.DistanceAlpha * staff.CurrentDistance + DevidedOrderConstant.WorkloadBeta * workloadRatio + salt;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestStaff = staff;
                }
            }

            // Assign the order to the best staff found
            if (bestStaff != null)
            {
                if (!assignedStaffs.TryGetValue(bestStaff.ShopDeliveryStaff.Id, out var staff))
                {
                    bestStaff.ShopDeliveryStaff.CurrentTaskLoad++;
                    bestStaff.CurrentDistance++;
                    bestStaff.Orders.Add(order);
                    bestStaff.Waiting++;
                    bestStaff.Total = bestStaff.Waiting + bestStaff.Delivering + bestStaff.Failed + bestStaff.Successful;
                    bestStaff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                    {
                        Id = order.DormitoryId,
                        Name = order.DormitoryName,
                        Total = 1,
                        Waiting = 1,
                        Successful = 0,
                        Delivering = 0,
                        Failed = 0,
                        ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                        {
                            Id = bestStaff.ShopDeliveryStaff.Id,
                            FullName = bestStaff.ShopDeliveryStaff.FullName,
                            AvatarUrl = bestStaff.ShopDeliveryStaff.AvatarUrl,
                            IsShopOwner = bestStaff.ShopDeliveryStaff.IsShopOwner,
                        },
                    });
                    assignedStaffs.Add(bestStaff.ShopDeliveryStaff.Id, bestStaff);
                }
                else
                {
                    staff.ShopDeliveryStaff.CurrentTaskLoad++;
                    staff.Orders.Add(order);
                    staff.Waiting++;
                    staff.Total = staff.Waiting + staff.Delivering + staff.Failed + staff.Successful;

                    // Check if order located in order dormitory will inscrease currentDistance
                    if (staff.Dormitories.All(d => d.Id != order.DormitoryId))
                    {
                        staff.CurrentDistance++;
                    }

                    var dormitoryExist = staff.Dormitories.FirstOrDefault(d => d.Id == order.DormitoryId);
                    if (dormitoryExist != null)
                    {
                        staff.Dormitories.Remove(dormitoryExist);
                        dormitoryExist.Total++;
                        dormitoryExist.Waiting++;
                        staff.Dormitories.Add(dormitoryExist);
                    }
                    else
                    {
                        staff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                        {
                            Id = order.DormitoryId,
                            Name = order.DormitoryName,
                            Total = 1,
                            Waiting = 1,
                            Successful = 0,
                            Delivering = 0,
                            Failed = 0,
                            ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                            {
                                Id = staff.ShopDeliveryStaff.Id,
                                FullName = staff.ShopDeliveryStaff.FullName,
                                AvatarUrl = staff.ShopDeliveryStaff.AvatarUrl,
                                IsShopOwner = staff.ShopDeliveryStaff.IsShopOwner,
                            },
                        });
                    }

                    assignedStaffs.Remove(staff.ShopDeliveryStaff.Id);
                    assignedStaffs.Add(staff.ShopDeliveryStaff.Id, staff);
                }
            }
            else
            {
                unAssignOrder.Add(order);
            }
        }

        // Refill Staff When Have Not Enough Order
        if (assignedStaffs.Values.Count != listStaff.Count)
        {
            for (var i = 0; i < listStaff.Count; i++)
            {
                if (assignedStaffs.TryGetValue(listStaff[i].ShopDeliveryStaff.Id, out var staffAssigned))
                {
                    listStaff[i] = staffAssigned;
                }
            }

            return (listStaff, unAssignOrder);
        }

        return (assignedStaffs.Values.ToList(), unAssignOrder);
    }

    private DeliveryPackageForAssignResponse AssignOrderToStaff(DeliveryPackageForAssignResponse staff, List<OrderDetailForShopResponse> orders)
    {
        foreach (var order in orders)
        {
            staff.ShopDeliveryStaff.CurrentTaskLoad++;
            staff.Orders.Add(order);
            staff.Waiting++;
            staff.Total = staff.Waiting + staff.Delivering + staff.Failed + staff.Successful;

            // Check if order located in order dormitory will inscrease currentDistance
            if (staff.Dormitories.All(d => d.Id != order.DormitoryId))
            {
                staff.CurrentDistance++;
            }

            var dormitoryExist = staff.Dormitories.FirstOrDefault(d => d.Id == order.DormitoryId);
            if (dormitoryExist != null)
            {
                staff.Dormitories.Remove(dormitoryExist);
                dormitoryExist.Total++;
                dormitoryExist.Waiting++;
                staff.Dormitories.Add(dormitoryExist);
            }
            else
            {
                staff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                {
                    Id = order.DormitoryId,
                    Name = order.DormitoryName,
                    Total = 1,
                    Waiting = 1,
                    Successful = 0,
                    Delivering = 0,
                    Failed = 0,
                    ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                    {
                        Id = staff.ShopDeliveryStaff.Id,
                        FullName = staff.ShopDeliveryStaff.FullName,
                        AvatarUrl = staff.ShopDeliveryStaff.AvatarUrl,
                        IsShopOwner = staff.ShopDeliveryStaff.IsShopOwner,
                    },
                });
            }
        }

        return staff;
    }

    private async Task<List<DeliveryPackageForAssignResponse>> GetListShopDeliveryStaffAsync(SuggestAssignDeliveryPackageQuery request)
    {
        var response = await _dapperService.SelectAsync<DeliveryPackageForAssignResponse, DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignResponse>(
            QueryName.GetListShopDeliveryStaffToAssign,
            (parent, child1) =>
            {
                parent.ShopDeliveryStaff = child1;
                return parent;
            },
            new
            {
                IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().ToString("yyyy-MM-dd"),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            },
            "StaffInforSection").ConfigureAwait(false);

        return response.ToList();
    }

    private async Task<List<OrderDetailForShopResponse>> GetListOrderUnAssignByTimeFrameAsync(SuggestAssignDeliveryPackageQuery request)
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
                    IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().ToString("yyyy-MM-dd"),
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                },
                "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return uniqOrder.Values.ToList();
    }
}