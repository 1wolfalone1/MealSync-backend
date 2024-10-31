using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetDeliveryPackageGroupDetailByTimeFrames;

public class GetDeliveryPackageGroupDetailByTimeFrameHandler : IQueryHandler<GetDeliveryPackageGroupDetailByTimeFrameQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetDeliveryPackageGroupDetailByTimeFrameHandler(IDapperService dapperService, IDeliveryPackageRepository deliveryPackageRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _dapperService = dapperService;
        _deliveryPackageRepository = deliveryPackageRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetDeliveryPackageGroupDetailByTimeFrameQuery request, CancellationToken cancellationToken)
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

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse>(
            QueryName.GetListDeliveryDetailStasticByTimeFrame,
            map,
            new
            {
                IntendedReceiveDate = request.IntendedReceiveDate.ToString("yyyy-M-d"),
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

        var unassignOrder = await GetListOrderUnAssignByTimeFrameAsync(request).ConfigureAwait(false);

        return Result.Success(new DeliveryPackageIntervalResponse()
        {
            IntendedReceiveDate = request.IntendedReceiveDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DeliveryPackageGroups = deliveryPackages,
            UnassignOrders = unassignOrder,
        });
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderByDeliveryPackageIdAsync(long? packageId)
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
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                    DeliveryPackageId = packageId,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }

    private async Task<List<OrderForShopByStatusResponse>> GetListOrderUnAssignByTimeFrameAsync(GetDeliveryPackageGroupDetailByTimeFrameQuery request)
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
                    IntendedReceiveDate = request.IntendedReceiveDate.ToString("yyyy-M-d"),
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
                },
                "CustomerSection, ShopDeliverySection, FoodSection").ConfigureAwait(false);

        return orderUniq.Values.ToList();
    }
}