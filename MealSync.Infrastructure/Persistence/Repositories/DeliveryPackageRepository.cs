using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DeliveryPackageRepository : BaseRepository<DeliveryPackage>, IDeliveryPackageRepository
{
    public DeliveryPackageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public DeliveryPackage GetPackageByShipIdAndTimeFrame(bool isShopOwnerShip, long shipperId, int startTime, int endTime)
    {
        var result = DbSet.Where(dp => (isShopOwnerShip && dp.ShopId == shipperId
                                        || !isShopOwnerShip && dp.ShopDeliveryStaffId == shipperId)
                                       && dp.StartTime == startTime
                                       && dp.EndTime == endTime
                                       && dp.DeliveryDate.Date == TimeFrameUtils.GetCurrentDateInUTC7().Date)
            .Include(dp => dp.Orders).FirstOrDefault();
        return result;
    }

    public List<DeliveryPackage> GetPackagesByFrameAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId)
    {
        var result = DbSet.Include(dp => dp.Orders)
            .Where(dp => dp.StartTime == startTime
                         && dp.EndTime == endTime
                         && dp.DeliveryDate.Date == deliveryDate.Date
                         && dp.Orders.All(o => o.ShopId == shopId))
            .ToList();
        return result;
    }

    public List<(int StartTime, int EndTime)> GetTimeFramesByFrameIntervalAndDate(DateTime deliveryDate, int startTime, int endTime, long shopId)
    {
        var result = DbSet.Include(dp => dp.Orders)
            .Where(dp => dp.StartTime >= startTime
                         && dp.EndTime <= endTime
                         && dp.DeliveryDate.Date == deliveryDate.Date
                         && dp.Orders.All(o => o.ShopId == shopId))
            .GroupBy(o => new { o.StartTime, o.EndTime })
            .Select(g => new
            {
                g.Key.StartTime,
                g.Key.EndTime,
            })
            .ToList();

        return result.Select(x => (x.StartTime, x.EndTime)).OrderBy(x => x.StartTime).ToList();
    }

    public List<DeliveryPackage> GetAllDeliveryPackageInDate(DateTime deliveryDate, bool isShopOwnerShip, long shipperId)
    {
        var result = DbSet.Where(dp => dp.DeliveryDate.Date == deliveryDate.Date
                                       && dp.Status != DeliveryPackageStatus.Done
                                       && (isShopOwnerShip && dp.ShopId == shipperId ||
                                           dp.ShopDeliveryStaffId == shipperId)).ToList();
        return result;
    }

    public List<DeliveryPackage> GetAllRequestUpdate(DateTime deliveryDate, int startTime, int endTime, List<long> staffIds)
    {
        var result = DbSet.Where(dp => dp.DeliveryDate.Date == deliveryDate.Date
                                       && dp.StartTime == startTime && dp.EndTime == endTime
                                       && (dp.ShopId.HasValue && staffIds.Contains(dp.ShopId.Value) ||
                                           dp.ShopDeliveryStaffId.HasValue && staffIds.Contains(dp.ShopDeliveryStaffId.Value))).ToList();

        return result;
    }
}