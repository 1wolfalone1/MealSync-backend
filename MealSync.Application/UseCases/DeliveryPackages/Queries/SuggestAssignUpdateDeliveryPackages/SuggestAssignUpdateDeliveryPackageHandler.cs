using System.Net;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.SuggestAssignUpdateDeliveryPackages;

public class SuggestAssignUpdateDeliveryPackageHandler : IQueryHandler<SuggestAssignUpdateDeliveryPackageQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<SuggestAssignUpdateDeliveryPackageHandler> _logger;

    public SuggestAssignUpdateDeliveryPackageHandler(IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IShopRepository shopRepository, ILogger<SuggestAssignUpdateDeliveryPackageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _shopRepository = shopRepository;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(SuggestAssignUpdateDeliveryPackageQuery request, CancellationToken cancellationToken)
    {
        // Validate
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
        var shopStaffAlreadySave = await GetListDeliveryPackageAlreadyAssign(request).ConfigureAwait(false);

        // Add already data to list staff
        for (int i = 0; i < shopStaffs.Count; i++)
        {
            var shopStaffWithInfor = shopStaffAlreadySave.Where(sf => sf.ShopDeliveryStaff.Id == shopStaffs[i].ShopDeliveryStaff.Id).SingleOrDefault();
            if (shopStaffWithInfor != default)
            {
                shopStaffs[i] = shopStaffWithInfor;
            }
        }

        // Check if shop not ship remove shop
        var shopStaffsRequestAssign = shopStaffs.Where(sf => request.ShipperIds.Contains(sf.ShopDeliveryStaff.Id)).ToList();


        // Assign Section
        var assignOrder = AssignOrderByFormular(shopStaffsRequestAssign, shopStaffs, unAssignOrders, request.StaffMaxCarryWeight);

        return Result.Success(new
        {
            IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DeliveryPackageGroups = assignOrder.AssignedStaff,
            UnassignOrders = assignOrder.UnAssignOrder,
        });
    }

    private (List<DeliveryPackageForAssignUpdateResponse> AssignedStaff, List<OrderDetailForShopResponse> UnAssignOrder) AssignOrderByFormular(List<DeliveryPackageForAssignUpdateResponse> listStaffRequest,
        List<DeliveryPackageForAssignUpdateResponse> listStaffOrigin, List<OrderDetailForShopResponse> orders, double staffMaxWeightCarry)
    {
        List<OrderDetailForShopResponse> unAssignOrder = new();
        Dictionary<long, DeliveryPackageForAssignUpdateResponse> assignedStaffs = new();
        foreach (var order in orders)
        {
            DeliveryPackageForAssignUpdateResponse bestStaff = null;
            var bestScore = staffMaxWeightCarry * DevidedOrderConstant.StaffMaxCapacity * 10;

            foreach (var staff in listStaffRequest)
            {
                /* If order need to delivery to different dormitory when compare to current dormitory in staff
                    need to check other staff have that dormitory is more than 50% totalOrder. If no let them take
                 */
                var salt = 0;
                if (staff.Dormitories.All(d => d.Id != order.DormitoryId) && staff.Total > 0)
                {
                    var staffWithMinTotalOrder = listStaffRequest.Where(s => s.Dormitories.Select(d => d.Id).Contains(order.DormitoryId)).OrderBy(d => d.Total).FirstOrDefault();
                    // It mean other staff have zero order
                    if (staffWithMinTotalOrder == default && listStaffRequest.Count > 1)
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
                    if (listStaffRequest.Any(s => s.CurrentTaskLoad < staffMaxWeightCarry))
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
                if (!assignedStaffs.TryGetValue(bestStaff.ShopDeliveryStaff.Id, out var staff))
                {
                    bestStaff.Orders.Add(order);
                    if (order.Status == (int)OrderStatus.Preparing)
                    {
                        bestStaff.Waiting++;
                    }else if (order.Status == (int)OrderStatus.Delivering)
                    {
                        bestStaff.Delivering++;
                    }

                    bestStaff.Total = bestStaff.Waiting + bestStaff.Delivering + bestStaff.Failed + bestStaff.Successful;

                    var dormitoryExist = bestStaff.Dormitories.FirstOrDefault(d => d.Id == order.DormitoryId);
                    if (dormitoryExist != null)
                    {
                        bestStaff.Dormitories.Remove(dormitoryExist);
                        dormitoryExist.Total++;

                        if (order.Status == (int)OrderStatus.Preparing)
                        {
                            dormitoryExist.Waiting++;
                        }else if (order.Status == (int)OrderStatus.Delivering)
                        {
                            dormitoryExist.Delivering++;
                        }

                        bestStaff.Dormitories.Add(dormitoryExist);
                    }
                    else
                    {
                        bestStaff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                        {
                            Id = order.DormitoryId,
                            Name = order.DormitoryName,
                            Total = 1,
                            Waiting = order.Status == (int)OrderStatus.Preparing ? 1 : 0,
                            Successful = 0,
                            Delivering = order.Status == (int)OrderStatus.Delivering ? 1 : 0,
                            Failed = 0,
                            ShopDeliveryStaff = new DeliveryPackageForAssignResponse.ShopStaffInforResponse()
                            {
                                Id = bestStaff.ShopDeliveryStaff.Id,
                                FullName = bestStaff.ShopDeliveryStaff.FullName,
                                AvatarUrl = bestStaff.ShopDeliveryStaff.AvatarUrl,
                                IsShopOwner = bestStaff.ShopDeliveryStaff.IsShopOwner,
                            },
                        });
                    }

                    assignedStaffs.Add(bestStaff.ShopDeliveryStaff.Id, bestStaff);
                }
                else
                {
                    staff.Orders.Add(order);
                    if (order.Status == (int)OrderStatus.Preparing)
                    {
                        staff.Waiting++;
                    }else if (order.Status == (int)OrderStatus.Delivering)
                    {
                        staff.Delivering++;
                    }

                    staff.Total = staff.Waiting + staff.Delivering + staff.Failed + staff.Successful;

                    var dormitoryExist = staff.Dormitories.FirstOrDefault(d => d.Id == order.DormitoryId);
                    if (dormitoryExist != null)
                    {
                        staff.Dormitories.Remove(dormitoryExist);
                        dormitoryExist.Total++;

                        if (order.Status == (int)OrderStatus.Preparing)
                        {
                            dormitoryExist.Waiting++;
                        }else if (order.Status == (int)OrderStatus.Delivering)
                        {
                            dormitoryExist.Delivering++;
                        }

                        // DormitoryExist.Waiting++;
                        staff.Dormitories.Add(dormitoryExist);
                    }
                    else
                    {
                        staff.Dormitories.Add(new DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff()
                        {
                            Id = order.DormitoryId,
                            Name = order.DormitoryName,
                            Total = 1,
                            Waiting = order.Status == (int)OrderStatus.Preparing ? 1 : 0,
                            Successful = 0,
                            Delivering = order.Status == (int)OrderStatus.Delivering ? 1 : 0,
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
        if (assignedStaffs.Values.Count != listStaffOrigin.Count)
        {
            for (var i = 0; i < listStaffOrigin.Count; i++)
            {
                if (assignedStaffs.TryGetValue(listStaffOrigin[i].ShopDeliveryStaff.Id, out var staffAssigned))
                {
                    listStaffOrigin[i] = staffAssigned;
                }
            }

            return (listStaffOrigin, unAssignOrder);
        }

        return (assignedStaffs.Values.ToList(), unAssignOrder);
    }

    private void Validate(SuggestAssignUpdateDeliveryPackageQuery request)
    {
        var deliveryPackage = _deliveryPackageRepository.GetPackagesByFrameAndDate(TimeFrameUtils.GetCurrentDateInUTC7().Date, request.StartTime, request.EndTime, _currentPrincipalService.CurrentPrincipalId.Value);
        if (deliveryPackage == null || deliveryPackage.Count == 0)
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_NOT_CREATE_YET_FOR_AUTO_ASSIGN_UPDATE.GetDescription(), new object[] { TimeFrameUtils.GetTimeFrameString(request.StartTime, request.EndTime) });

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

    private async Task<List<DeliveryPackageForAssignUpdateResponse>> GetListDeliveryPackageAlreadyAssign(SuggestAssignUpdateDeliveryPackageQuery request)
    {
        var dicDormitoryUniq = new Dictionary<long, DeliveryPackageForAssignUpdateResponse>();
        Func<DeliveryPackageForAssignUpdateResponse, DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignUpdateResponse> map =
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

        await _dapperService.SelectAsync<DeliveryPackageForAssignUpdateResponse, DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignUpdateResponse>(
            QueryName.GetListDeliveryDetailStasticByTimeFrame,
            map,
            new
            {
                IntendedReceiveDate = TimeFrameUtils.GetCurrentDateInUTC7().ToString("yyyy-MM-dd"),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            },
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackages = dicDormitoryUniq.Values.ToList();
        foreach (var deliveryPackage in deliveryPackages)
        {
            deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId).ConfigureAwait(false);
        }

        return dicDormitoryUniq.Values.ToList();
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

    private async Task<List<DeliveryPackageForAssignUpdateResponse>> GetListShopDeliveryStaffAsync(SuggestAssignUpdateDeliveryPackageQuery request)
    {
        var response = await _dapperService.SelectAsync<DeliveryPackageForAssignUpdateResponse, DeliveryPackageForAssignResponse.ShopStaffInforResponse, DeliveryPackageForAssignUpdateResponse>(
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

    private async Task<List<OrderDetailForShopResponse>> GetListOrderUnAssignByTimeFrameAsync(SuggestAssignUpdateDeliveryPackageQuery request)
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