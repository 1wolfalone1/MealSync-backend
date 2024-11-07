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

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListDeliveryPackageForShopWebs;

public class GetListDeliveryPackageForShopWebHandler : IQueryHandler<GetListDeliveryPackageForShopWebQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetListDeliveryPackageForShopWebHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetListDeliveryPackageForShopWebQuery request, CancellationToken cancellationToken)
    {
        var deliveryPackages = _deliveryPackageRepository.GetTimeFramesByFrameIntervalAndDatePaging(request.PageIndex, request.PageSize, request.IntendedReceiveDate, request.StartTime, request.EndTime,
            _currentPrincipalService.CurrentPrincipalId.Value, request.DeliveryPackageId, request.DeliveryShopStaffFullName);

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