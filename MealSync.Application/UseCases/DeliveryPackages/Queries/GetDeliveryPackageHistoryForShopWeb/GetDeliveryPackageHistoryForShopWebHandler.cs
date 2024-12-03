using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.DeliveryPackages.Models;
using MealSync.Application.UseCases.DeliveryPackages.Queries.GetAllDeliveryPackages;
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
}