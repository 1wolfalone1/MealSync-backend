using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IDeliveryPackageRepository : IBaseRepository<DeliveryPackage>
{
    DeliveryPackage GetPackageByShipIdAndTimeFrame(bool isShopOwnerShip, long shipperId, int startTime, int endTime);

    List<DeliveryPackage> GetPackagesByFrameAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId);

    List<(int StartTime, int EndTime)> GetTimeFramesByFrameIntervalAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId);

    List<DeliveryPackage> GetAllDeliveryPackageInDate(DateTime deliveryDate, bool isShopOwnerShip, long shipperId, DeliveryPackageStatus[] status);

    List<DeliveryPackage> GetAllRequestUpdate(DateTime deliveryDate, int startTime, int endTime, List<long> staffIds);

    (int Total, List<DeliveryPackage> DeliveryPackages) GetTimeFramesByFrameIntervalAndDatePaging(int pageIndex, int pageSize, DateTime deliveryDate, int startTime, int endTime, long shopId, string? deliveryPackageId, string? fullName);
}