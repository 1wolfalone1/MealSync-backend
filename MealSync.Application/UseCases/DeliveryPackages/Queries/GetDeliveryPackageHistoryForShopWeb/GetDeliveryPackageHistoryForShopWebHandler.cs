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

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageHistoryForShopWeb;

public class GetDeliveryPackageHistoryForShopWebHandler : IQueryHandler<GetDeliveryPackageHistoryForShopWebQuery, Result>
{
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDapperService _dapperService;

    public GetDeliveryPackageHistoryForShopWebHandler(IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService, IDapperService dapperService)
    {
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetDeliveryPackageHistoryForShopWebQuery request, CancellationToken cancellationToken)
    {
        var deliveryPackages = _deliveryPackageRepository.GetDeliveryPackageHistoryFilterForShopWeb(request.SearchValue, _currentPrincipalService.CurrentPrincipalId.Value, (int)request.StatusMode, request.DateFrom,
            request.DateTo, request.PageIndex, request.PageSize);
        var deliveryPackageResponse = new List<DeliveryPackageGroupDetailForWebResponse>();
        foreach (var deliveryPackage in deliveryPackages.DeliveryPackages)
        {
            var dp = await GetDeliveryPackageStatisticAsync(deliveryPackage.Id).ConfigureAwait(false);
            if (dp != null)
            {
                dp.IntenededReceiveDate = deliveryPackage.DeliveryDate;
                dp.StartTime = dp.StartTime;
                dp.EndTime = dp.EndTime;
                dp.Status = dp.Status;
                dp.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.Id, _currentPrincipalService.CurrentPrincipalId.Value).ConfigureAwait(false);
                deliveryPackageResponse.Add(dp);
            }
            else
            {
                deliveryPackages.TotalCount--;
            }
        }

        return Result.Success(new PaginationResponse<DeliveryPackageGroupDetailForWebResponse>(deliveryPackageResponse, deliveryPackages.TotalCount, request.PageIndex, request.PageSize));
    }

    private async Task<DeliveryPackageGroupDetailForWebResponse> GetDeliveryPackageStatisticAsync(long id)
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