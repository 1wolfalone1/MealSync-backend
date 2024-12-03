using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageDetailByTimeFrames;

public class GetDeliveryPackageDetailHandler : IQueryHandler<GetDeliveryPackageDetailQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;

    public GetDeliveryPackageDetailHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, ICurrentAccountService currentAccountService, IShopDeliveryStaffRepository shopDeliveryStaffRepository)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentAccountService = currentAccountService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
    }

    public async Task<Result<Result>> Handle(GetDeliveryPackageDetailQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var account = _currentAccountService.GetCurrentAccount();
        var deliveryPackage = _deliveryPackageRepository.GetById(request.Id);
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var response = await GetDeliveryPackageStatisticsAsync(deliveryPackage.StartTime, deliveryPackage.EndTime, deliveryPackage.DeliveryDate, shopId, deliveryPackage.ShopDeliveryStaffId, new DeliveryPackageStatus[]
        {
            DeliveryPackageStatus.Created, DeliveryPackageStatus.Done, DeliveryPackageStatus.OnGoing
        }).ConfigureAwait(false);

        return Result.Success(response);
    }

    private async Task<DeliveryPackageGroupDetailForMobileResponse> GetDeliveryPackageStatisticsAsync(int startTime, int endTime, DateTime deliveryDate, long shopId, long? shopDeliveryStaffId, DeliveryPackageStatus[] status)
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
            IntendedReceiveDate = deliveryDate.ToString("yyyy-MM-dd"),
            ShopDeliveryStaffId = shopDeliveryStaffId.HasValue ? shopDeliveryStaffId.Value : (long?)null,
            IsShopOwnerShip = !shopDeliveryStaffId.HasValue,
            Status = status,
            ShopId = shopId,
            StartTime = startTime,
            EndTime = endTime,
        };

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailForMobileResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailForMobileResponse>(
            QueryName.GetAllDeliveryPackageStasisticByTimeFrame,
            map,
            parameter,
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackage = dicDormitoryUniq.Values.ToList().FirstOrDefault();

        deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId, shopId).ConfigureAwait(false);

        return deliveryPackage;
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

    private void Validate(GetDeliveryPackageDetailQuery request)
    {
        var isShopOwner = _currentAccountService.GetCurrentAccount().RoleId == (int)Domain.Enums.Roles.ShopOwner;
        if (!_deliveryPackageRepository.CheckIsExistDeliveryPackageBaseOnRole(isShopOwner, request.Id, _currentAccountService.GetCurrentAccount().Id))
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_NOT_FOUND.GetDescription(), new object[] { request.Id });
    }
}