using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Map;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignDeliveryPackages;

public class SuggestAssignDeliveryPackageHandler : IQueryHandler<SuggestAssignDeliveryPackageQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapApiService _mapApiService;
    private readonly IShopRepository _shopRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IDormitoryRepository _dormitoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SuggestAssignDeliveryPackageHandler> _logger;

    public SuggestAssignDeliveryPackageHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IMapApiService mapApiService, IShopRepository shopRepository, IBuildingRepository buildingRepository, IDormitoryRepository dormitoryRepository, IUnitOfWork unitOfWork, ILogger<SuggestAssignDeliveryPackageHandler> logger)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _mapApiService = mapApiService;
        _shopRepository = shopRepository;
        _buildingRepository = buildingRepository;
        _dormitoryRepository = dormitoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(SuggestAssignDeliveryPackageQuery request, CancellationToken cancellationToken)
    {
        // Valiate
        Validate(request);

        // Save shop max carry weight
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
            shop.MaxCarryWeight = request.StaffMaxCarryWeight;
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var unAssignOrders = await GetListOrderUnAssignByTimeFrameAsync(request).ConfigureAwait(false);
        var shopStaffs = await GetListShopDeliveryStaffAsync(request).ConfigureAwait(false);

        // Check if shop not ship remove shop
        shopStaffs = shopStaffs.Where(sf => request.ShipperIds.Contains(sf.ShopDeliveryStaff.Id)).ToList();
        shopStaffs.ForEach(s =>
        {
            s.StartTime = request.StartTime;
            s.EndTime = request.EndTime;
        });

        var assignOrder = await AssignOrderByFormularAsync(shopStaffs, unAssignOrders, request.StaffMaxCarryWeight).ConfigureAwait(false);

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

    private async Task<(List<DeliveryPackageForAssignResponse> AssignedStaff, List<OrderDetailForShopResponse> UnAssignOrder)> AssignOrderByFormularAsync(List<DeliveryPackageForAssignResponse> listStaff,
        List<OrderDetailForShopResponse> orders, double staffMaxWeightCarry)
    {
        List<OrderDetailForShopResponse> unAssignOrder = new();
        Dictionary<long, DeliveryPackageForAssignResponse> assignedStaffs = new();
        var shop = _shopRepository.Get(s => s.Id == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(s => s.Location).SingleOrDefault();
        var shopLocation = shop.Location;
        foreach (var order in orders)
        {
            DeliveryPackageForAssignResponse bestStaff = null;
            var bestScore = staffMaxWeightCarry * DevidedOrderConstant.StaffMaxCapacity * 10;

            foreach (var staff in listStaff)
            {
                /* If order need to delivery to different dormitory when compare to current dormitory in staff
                    need to check other staff have that dormitory is more than 50% totalOrder. If no let them take
                 */
                var salt = 0;
                if (staff.Dormitories.All(d => d.Id != order.DormitoryId) && staff.Total > 0)
                {
                    var staffWithMinTotalOrder = listStaff.Where(s => s.Dormitories.Select(d => d.Id).Contains(order.DormitoryId)).OrderBy(d => d.Total).FirstOrDefault();
                    // It mean other staff have zero order
                    if (staffWithMinTotalOrder == default && listStaff.Count > 1)
                    {
                        salt = int.MaxValue;
                    }
                    // It mean order staff have order so need to check number of task
                    else if (staffWithMinTotalOrder != default)
                    {
                        var percentTaskCompare = staffWithMinTotalOrder.Total / staff.Total;
                        if (percentTaskCompare < DevidedOrderConstant.PercentOverTaskCanAccept)
                        {
                            salt = int.MaxValue;
                        }
                    }
                }

                // Check time
                if (staff.Total > 0 && staff.TotalMinutesHandleDelivery >= FrameConstant.TIME_FRAME_IN_MINUTES)
                {
                    // Check if time wait customer + min time move to other dormitory over 30p will not let it receive
                    var minTime = staff.Dormitories.Min(d => d.MinutesMoveTo);
                    if (staff.TotalMinutesHandleDelivery - minTime >= (FrameConstant.TIME_FRAME_IN_MINUTES + DevidedOrderConstant.MinutesAddWhenOrderMoreThanFive))
                    {
                        salt = int.MaxValue;
                    }
                }

                if (staff.CurrentTaskLoad > staffMaxWeightCarry)
                {
                    // Check have any staff can carry this order
                    if (listStaff.Any(s => s.CurrentTaskLoad < staffMaxWeightCarry))
                    {
                        salt = int.MaxValue;
                    }
                    else
                    {
                        // If no staff can carry this order. Will check variant 1 kg + current taskload
                        if ((staff.CurrentTaskLoad + order.TotalWeight) > (staffMaxWeightCarry + DevidedOrderConstant.VariantKgCanBeAccept))
                        {
                            salt = int.MaxValue;
                        }
                    }
                }

                // Calculate the score (lower score is better)
                var workloadRatio = staff.CurrentTaskLoad / staffMaxWeightCarry;
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
                // New staff
                if (!assignedStaffs.TryGetValue(bestStaff.ShopDeliveryStaff.Id, out var staff))
                {
                    // Need call google to calculate time and distance to delivery
                    var dormitoryLocation = new Location()
                    {
                        Latitude = order.Customer.Latitude,
                        Longitude = order.Customer.Longitude,
                        Address = order.Customer.Address,
                    };

                    var element = await _mapApiService.GetDistanceOneDestinationAsync(shopLocation, dormitoryLocation, VehicleMaps.Car).ConfigureAwait(false);
                    bestStaff.CurrentDistance = element.Distance.Value / 1000; // Convert to km
                    bestStaff.TotalMinutestToMove += ((int)element.Duration.Value / 60);

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
                        MinutesMoveTo = element.Duration.Value / 60,
                        ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                        {
                            Id = bestStaff.ShopDeliveryStaff.Id,
                            FullName = bestStaff.ShopDeliveryStaff.FullName,
                            AvatarUrl = bestStaff.ShopDeliveryStaff.AvatarUrl,
                            IsShopOwner = bestStaff.ShopDeliveryStaff.IsShopOwner,
                        },
                    });

                    bestStaff = await CalculateAverageWaitForStaff(bestStaff).ConfigureAwait(false);
                    assignedStaffs.Add(bestStaff.ShopDeliveryStaff.Id, bestStaff);
                }
                else
                {
                    staff.Orders.Add(order);
                    staff.Waiting++;
                    staff.Total = staff.Waiting + staff.Delivering + staff.Failed + staff.Successful;

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
                        // Need call google to calculate time and distance to delivery
                        var dormitoryLocation = _dormitoryRepository.GetLocationByDormitoryId(order.DormitoryId);

                        var buildingLocation = _dormitoryRepository.GetLocationByDormitoryId(staff.Dormitories[staff.Dormitories.Count - 1].Id);
                        var element = await _mapApiService.GetDistanceOneDestinationAsync(buildingLocation, dormitoryLocation, VehicleMaps.Car).ConfigureAwait(false);
                        staff.CurrentDistance += element.Distance.Value / 1000; // Convert to km
                        staff.TotalMinutestToMove += ((int)element.Duration.Value / 60);

                        staff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                        {
                            Id = order.DormitoryId,
                            Name = order.DormitoryName,
                            Total = 1,
                            Waiting = 1,
                            Successful = 0,
                            Delivering = 0,
                            Failed = 0,
                            MinutesMoveTo = element.Duration.Value / 60,
                            ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                            {
                                Id = staff.ShopDeliveryStaff.Id,
                                FullName = staff.ShopDeliveryStaff.FullName,
                                AvatarUrl = staff.ShopDeliveryStaff.AvatarUrl,
                                IsShopOwner = staff.ShopDeliveryStaff.IsShopOwner,
                            },
                        });
                    }

                    staff = await CalculateAverageWaitForStaff(staff).ConfigureAwait(false);
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

    private async Task<DeliveryPackageForAssignResponse> CalculateAverageWaitForStaff(DeliveryPackageForAssignResponse staff)
    {
        var orderGroupBy = staff.Orders.GroupBy(o => o.DormitoryId)
            .ToDictionary(g => g.Key, g => g.Select(o => o.BuildingId).Distinct().ToList());
        int average = 0;
        foreach (var order in orderGroupBy)
        {
            var originLocation = _dormitoryRepository.GetLocationByDormitoryId(order.Key);
            var destinations = _buildingRepository.GetListLocationBaseOnBuildingIds(order.Value.ToList());
            average += await AverageTimeToWaitCustomer(originLocation, destinations).ConfigureAwait(false) / 60;
        }

        staff.TotalMinutesToWaitCustomer = average;
        return staff;
    }

    private async Task<int> AverageTimeToWaitCustomer(Location origin, List<Location> destinations)
    {
        var matrix = await _mapApiService.GetDistanceMatrixAsync(origin, destinations, VehicleMaps.Bike).ConfigureAwait(false);
        if (matrix != null)
        {
            return matrix.Rows.First().AverageDuration;
        }

        return 5 * 60;
    }

    private DeliveryPackageForAssignResponse AssignOrderToStaff(DeliveryPackageForAssignResponse staff, List<OrderDetailForShopResponse> orders)
    {
        foreach (var order in orders)
        {
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