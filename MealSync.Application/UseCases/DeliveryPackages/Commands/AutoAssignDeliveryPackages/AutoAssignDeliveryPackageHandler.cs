using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.DeliveryPackages.Commands.AutoAssignDeliveryPackages;

public class AutoAssignDeliveryPackageHandler : ICommandHandler<AutoAssignDeliveryPackageCommand, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public AutoAssignDeliveryPackageHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(AutoAssignDeliveryPackageCommand request, CancellationToken cancellationToken)
    {
        // Valiate
        Validate(request);

        var unAssignOrders = await GetListOrderUnAssignByTimeFrameAsync(request).ConfigureAwait(false);
        var shopStaffs = await GetListShopDeliveryStaffAsync(request).ConfigureAwait(false);

        // Check if shop not ship remove shop
        if (!request.IsShopOwnerShip)
        {
            shopStaffs = shopStaffs.Where(sf => !sf.ShopDeliveryStaff.IsShopOwner).ToList();
        }

        var assignOrder = AssignOrder(shopStaffs, unAssignOrders);
        return Result.Success(new
        {
            DeliverPackageGroup = assignOrder.AssignedStaff,
            UnassignOrder = assignOrder.UnAssignOrder,
        });
    }

    private void Validate(AutoAssignDeliveryPackageCommand request)
    {
        var deliveryPackage = _deliveryPackageRepository.GetPackagesByFrameAndDate(TimeFrameUtils.GetCurrentDateInUTC7().Date, request.StartTime, request.EndTime, _currentPrincipalService.CurrentPrincipalId.Value);
        if (deliveryPackage != null && deliveryPackage.Count > 0)
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_TIME_FRAME_CREATED.GetDescription(), new object[] { TimeFrameUtils.GetTimeFrameString(request.StartTime, request.EndTime) });
    }

    private (List<DeliveryPackageForAssignResponse> AssignedStaff, List<OrderForShopByStatusResponse> UnAssignOrder) AssignOrder(List<DeliveryPackageForAssignResponse> listStaff, List<OrderForShopByStatusResponse> orders)
    {
        var ordersByDormitory = orders.GroupBy(o => o.DormitoryId)
            .ToDictionary(g => g.Key, g => (g.Count(), g.ToList()));

        // Calculate the ideal number of orders each staff should handle
        double equallyOrderForEachStaff = Math.Round((orders.Count / listStaff.Count * 1.0), MidpointRounding.ToEven);

        int staffIndex = 0;
        List<(long DormitoryId, long NumberOfOrder)> dormitoryIdsSatisfyCondition = new();
        foreach (var orderDormitory in ordersByDormitory)
        {
            if (orderDormitory.Value.Item1 <= equallyOrderForEachStaff)
            {
                dormitoryIdsSatisfyCondition.Add((orderDormitory.Key, orderDormitory.Value.Item1));
            }
        }

        for (int i = 0; i < listStaff.Count(); i++)
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

        return (listStaff, new List<OrderForShopByStatusResponse>());
    }

    private (List<DeliveryPackageForAssignResponse> AssignedStaff, List<OrderForShopByStatusResponse> UnAssignOrder) AssignOrderByFormular(List<DeliveryPackageForAssignResponse> listStaff, List<OrderForShopByStatusResponse> orders)
    {
        List<OrderForShopByStatusResponse> unAssignOrder = new();
        Dictionary<long, DeliveryPackageForAssignResponse> assignedStaffs = new();
        foreach (var order in orders)
        {
            DeliveryPackageForAssignResponse bestStaff = null;
            double bestScore = double.MaxValue;

            foreach (var staff in listStaff)
            {
                // Calculate workload ratio
                double workloadRatio = staff.ShopDeliveryStaff.CurrentTaskLoad / DevidedOrderConstant.StaffMaxCapacity;

                /* If order need to delivery to different dormitory when compare to current dormitory in staff
                    need to check other staff have that dormitory is more than 50% totalOrder. If no let them take
                 */
                int salt = 0;
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
                double score = DevidedOrderConstant.DistanceAlpha * staff.CurrentDistance + DevidedOrderConstant.WorkloadBeta * workloadRatio + salt;

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
            for (int i = 0; i < listStaff.Count; i++)
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

    private DeliveryPackageForAssignResponse AssignOrderToStaff(DeliveryPackageForAssignResponse staff, List<OrderForShopByStatusResponse> orders)
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

    private async Task<List<DeliveryPackageForAssignResponse>> GetListShopDeliveryStaffAsync(AutoAssignDeliveryPackageCommand request)
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
                IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().ToString("yyyy-M-d"),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            },
            "StaffInforSection").ConfigureAwait(false);

        return response.ToList();
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderUnAssignByTimeFrameAsync(AutoAssignDeliveryPackageCommand request)
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
                    IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().ToString("yyyy-M-d"),
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }
}