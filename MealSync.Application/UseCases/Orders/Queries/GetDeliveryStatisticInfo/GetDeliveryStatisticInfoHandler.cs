using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Map;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryStatisticInfo;

public class GetDeliveryStatisticInfoHandler : ICommandHandler<GetDeliveryStatisticInfoQuery, Result>
{
    private readonly IDapperService _dapperService;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopRepository _shopRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapApiService _apiService;
    private readonly IAccountRepository _accountRepository;
    private readonly IDormitoryDistanceRepository _dormitoryDistanceRepository;

    public GetDeliveryStatisticInfoHandler(IDapperService dapperService, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IShopRepository shopRepository, IShopDormitoryRepository shopDormitoryRepository, IMapApiService apiService, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IAccountRepository accountRepository, IDormitoryDistanceRepository dormitoryDistanceRepository)
    {
        _dapperService = dapperService;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _shopRepository = shopRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _apiService = apiService;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _accountRepository = accountRepository;
        _dormitoryDistanceRepository = dormitoryDistanceRepository;
    }

    public async Task<Result<Result>> Handle(GetDeliveryStatisticInfoQuery request, CancellationToken cancellationToken)
    {
        var orders = (await _dapperService.SelectAsync<OrderDetailsCalculateSuggestTimeResponse>(QueryName.GetListOrderForCalculateSuggestTime,
            new
            {
                OrderIds = request.OrderIds,
            }).ConfigureAwait(false)).ToList();

        int suggestStartTimeDelivery = 0;
        int totalMinutesHandleDelivery = 0;
        double currentDistance = 0;
        double currentTaskLoad = 0;
        double deliveryPackageWeight = 0;
        int totalMinutestToMove = 0;
        int totalMinutesToWaitCustomer = 0;
        var ordersGroup = orders.GroupBy(o => o.DormitoryId).ToDictionary(g => g.Key, g => g.ToList());
        var account = _accountRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        var shopLocationsDormitory = await _shopDormitoryRepository.GetByShopId(shopId).ConfigureAwait(false);
        var shopLocation = await _shopRepository.GetByIdIncludeLocation(shopId).ConfigureAwait(false);
        var dormitoryNeedToDelivery = new List<long>();
        foreach (var orderKey in ordersGroup)
        {
            // Delivery Weight
            deliveryPackageWeight += orderKey.Value.ToList().Sum(o => o.Weight);
            currentTaskLoad = deliveryPackageWeight;

            // Time move and distance
            if (dormitoryNeedToDelivery.Count == 0)
            {
                var shopDormitory = shopLocationsDormitory.Where(sd => sd.DormitoryId == orderKey.Key).FirstOrDefault();
                if (shopDormitory != null)
                {
                    currentDistance += shopDormitory.Distance;
                    totalMinutestToMove += shopDormitory.Duration;
                    dormitoryNeedToDelivery.Add(orderKey.Key);
                }
            }
            else
            {
                var dormitory = _dormitoryDistanceRepository.GetByIds(orderKey.Key, dormitoryNeedToDelivery[dormitoryNeedToDelivery.Count - 1]);
                totalMinutestToMove += dormitory.Duration;
                currentDistance += dormitory.Distance;
            }

            // Minutes wait customer
            var customerLocation = orderKey.Value.Select(o => new Location()
            {
                Address = o.CustomerAddress,
                Latitude = o.CustomerLatitude,
                Longitude = o.CustomerLongitude,
            }).ToList();
            totalMinutesToWaitCustomer += await AverageTimeToWaitCustomer(shopLocation.Location, customerLocation).ConfigureAwait(false);
        }

        var totalNumberOrderInDeliveringAndPreparing = orders.Count();
        totalMinutesHandleDelivery = (int)(totalMinutesToWaitCustomer + totalNumberOrderInDeliveringAndPreparing * 0.5 + totalMinutestToMove);
        var endTime = TimeFrameUtils.GetStartTimeToDateTime(DateTime.Now, request.EndTime);
        endTime = endTime.AddMinutes(-totalMinutesHandleDelivery);
        suggestStartTimeDelivery = int.Parse(endTime.ToString("HHmm"));

        return Result.Success(new
        {
            SuggestStartTimeDelivery = suggestStartTimeDelivery,
            TotalMinutesHandleDelivery = totalMinutesHandleDelivery,
            CurrentDistance = currentDistance,
            CurrentTaskLoad = currentTaskLoad,
            DeliveryPackageWeight = deliveryPackageWeight,
            TotalMinutestToMove = totalMinutestToMove,
            TotalMinutesToWaitCustomer = totalMinutesToWaitCustomer,
        });
    }

    private async Task<int> AverageTimeToWaitCustomer(Location origin, List<Location> destinations)
    {
        var currentDate = TimeFrameUtils.GetCurrentDateInUTC7();

        // In hot time
        if (currentDate.Hour >= 11 && currentDate.Hour <= 13)
        {
            return 8;
        }

        return 5;
    }
}