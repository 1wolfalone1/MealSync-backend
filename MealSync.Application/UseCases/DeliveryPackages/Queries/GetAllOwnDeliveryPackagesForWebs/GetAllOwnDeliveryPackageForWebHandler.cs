using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackages;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllOwnDeliveryPackagesForWebs;

public class GetAllOwnDeliveryPackageForWebHandler : IQueryHandler<GetAllOwnDeliveryPackageForWebQuery, Result>
{
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IDapperService _dapperService;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetAllOwnDeliveryPackageForWebHandler(ICurrentAccountService currentAccountService, IDapperService dapperService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _currentAccountService = currentAccountService;
        _dapperService = dapperService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetAllOwnDeliveryPackageForWebQuery request, CancellationToken cancellationToken)
    {
        var deliveryPackages = _deliveryPackageRepository.GetAllOwnDeliveryPackageFilter(request.PageIndex, request.PageSize, request.IntendedReceiveDate, request.StartTime, request.EndTime, request.Status, request.DeliveryPackageId,
            _currentPrincipalService.CurrentPrincipalId.Value);

        var deliveryPackageResponse = new List<DeliveryPackageGroupDetailForWebResponse>();
        foreach (var deliveryPackage in deliveryPackages.DeliveryPackages)
        {
            deliveryPackageResponse.Add(await GetDeliveryPackageStatsisticAsync(deliveryPackage.Id).ConfigureAwait(false));
        }

        var response = new PaginationResponse<DeliveryPackageGroupDetailForWebResponse>(deliveryPackageResponse, deliveryPackages.Total, request.PageIndex, request.PageSize);
        return Result.Success(response);
    }

    private async Task<DeliveryPackageGroupDetailForWebResponse> GetDeliveryPackageStatsisticAsync(long id)
    {
        var dicDormitoryUniq = new Dictionary<long, DeliveryPackageGroupDetailForWebResponse>();
        Func<DeliveryPackageGroupDetailForWebResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailForWebResponse> map =
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

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailForWebResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailForWebResponse>(
            QueryName.GetDeliveryPackackageStasisticById,
            map,
            new
            {
                DeliveryPackageId = id,
            },
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackage = dicDormitoryUniq.Values.FirstOrDefault();
        if (deliveryPackage != null)
        {
            deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId, _currentPrincipalService.CurrentPrincipalId.Value).ConfigureAwait(false);
        }

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
}