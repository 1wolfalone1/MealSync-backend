using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackages;

public class GetAllDeliveryPackageHandler : IQueryHandler<GetAllDeliveryPackageQuery, Result>
{
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IDapperService _dapperService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public GetAllDeliveryPackageHandler(ICurrentAccountService currentAccountService, IDeliveryPackageRepository deliveryPackageRepository, IDapperService dapperService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _currentAccountService = currentAccountService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _dapperService = dapperService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(GetAllDeliveryPackageQuery request, CancellationToken cancellationToken)
    {
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var response = await GetDeliveryPackageStatsisticAsync(request, shopId, account.RoleId == (int)Domain.Enums.Roles.ShopDelivery ? account.Id : null, request.Status);
        return Result.Success(response);
    }

    private async Task<List<DeliveryPackageGroupDetailForMobileResponse>> GetDeliveryPackageStatsisticAsync(GetAllDeliveryPackageQuery request, long shopId, long? shopDeliveryStaffId, DeliveryPackageStatus[] status)
    {
        var dicDormitoryUniq = new Dictionary<long, DeliveryPackageGroupDetailForMobileResponse>();
        Func<DeliveryPackageGroupDetailForMobileResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailForMobileResponse> map =
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

        var parameter = new
        {
            IntendedReceiveDate = request.IntendedReceiveDate.ToString("yyyy-MM-dd"),
            ShopDeliveryStaffId = shopDeliveryStaffId.HasValue ? shopDeliveryStaffId.Value : (long?)null,
            IsShopOwnerShip = !shopDeliveryStaffId.HasValue,
            Status = status,
            ShopId = shopId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
        };

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailForMobileResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailForMobileResponse>(
            QueryName.GetAllDeliveryPackageStasisticByTimeFrame,
            map,
            parameter,
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackages = dicDormitoryUniq.Values.ToList();
        foreach (var deliveryPackage in deliveryPackages)
        {
            deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId, shopId).ConfigureAwait(false);
        }

        return deliveryPackages;
    }

    private async Task<List<OrderDetailForShopResponse>> GetListOrderByDeliveryPackageIdAsync(long? packageId, long shopId)
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
                    ShopId = shopId,
                    DeliveryPackageId = packageId,
                },
                "CustomerSection, PromotionSection, DeliveryPackageSection, OrderDetailSection").ConfigureAwait(false);

        return uniqOrder.Values.ToList();
    }
}