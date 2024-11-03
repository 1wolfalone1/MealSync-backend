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
        var response = await GetDeliveryPackageStatsisticAsync(deliveryPackage.StartTime, deliveryPackage.EndTime, deliveryPackage.DeliveryDate, shopId, deliveryPackage.ShopDeliveryStaffId, new DeliveryPackageStatus[]
        {
            DeliveryPackageStatus.Created, DeliveryPackageStatus.Done, DeliveryPackageStatus.OnGoing
        }).ConfigureAwait(false);

        return Result.Success(response);
    }

    private async Task<DeliveryPackageGroupDetailResponse> GetDeliveryPackageStatsisticAsync(int startTime, int endTime, DateTime deliveryDate, long shopId, long? shopDeliveryStaffId, DeliveryPackageStatus[] status)
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
            IntendedReceiveDate = deliveryDate.ToString("yyyy-MM-dd"),
            ShopDeliveryStaffId = shopDeliveryStaffId.HasValue ? shopDeliveryStaffId.Value : (long?)null,
            IsShopOwnerShip = !shopDeliveryStaffId.HasValue,
            Status = status,
            ShopId = shopId,
            StartTime = startTime,
            EndTime = endTime,
        };

        await _dapperService.SelectAsync<DeliveryPackageGroupDetailResponse, DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff,
            DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage, DeliveryPackageGroupDetailResponse>(
            QueryName.GetAllDeliveryPackageStasisticByTimeFrame,
            map,
            parameter,
            "ShopDeliverySection, DormitorySection, ShopDeliveryInDorSection").ConfigureAwait(false);

        var deliveryPackage = dicDormitoryUniq.Values.ToList().FirstOrDefault();

        deliveryPackage.Orders = await GetListOrderByDeliveryPackageIdAsync(deliveryPackage.DeliveryPackageId, shopId).ConfigureAwait(false);

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

    private void Validate(GetDeliveryPackageDetailQuery request)
    {
        var isShopOwner = _currentAccountService.GetCurrentAccount().RoleId == (int)Domain.Enums.Roles.ShopOwner;
        if (_deliveryPackageRepository.Get(dp => dp.Id == request.Id && (isShopOwner && dp.ShopId == _currentAccountService.GetCurrentAccount().Id ||
                                                                         !isShopOwner && dp.ShopDeliveryStaffId == _currentAccountService.GetCurrentAccount().Id)).SingleOrDefault() == default)
            throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}