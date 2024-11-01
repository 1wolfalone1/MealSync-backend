﻿using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IDeliveryPackageRepository : IBaseRepository<DeliveryPackage>
{
    DeliveryPackage GetPackageByShipIdAndTimeFrame(bool isShopOwnerShip, long shipperId, int startTime, int endTime);

    List<DeliveryPackage> GetPackagesByFrameAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId);

    List<(int StartTime, int EndTime)> GetTimeFramesByFrameIntervalAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId);

    List<DeliveryPackage> GetAllDeliveryPackageInDate(DateTime deliveryDate, bool isShopOwnerShip, long shipperId);

    List<DeliveryPackage> GetAllRequestUpdate(DateTime deliveryDate, int startTime, int endTime, List<long> staffIds);
}