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

    private async Task<List<DeliveryPackageGroupDetailResponse>> GetDeliveryPackageStatsisticAsync(GetAllDeliveryPackageQuery request, long shopId, long? shopDeliveryStaffId, DeliveryPackageStatus[] status)
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

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse>(
            QueryName.GetDeliveryPackageStasisticById,
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

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderByDeliveryPackageIdAsync(long? packageId, long shopId)
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
                    ShopId = shopId,
                    DeliveryPackageId = packageId,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }
}