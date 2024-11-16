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

    public (int Total, List<DeliveryPackage> DeliveryPackages) GetTimeFramesByFrameIntervalAndDatePaging(int pageIndex, int pageSize, DateTime deliveryDate, int startTime, int endTime, long shopId, string? deliveryPackageId, string? fullName)
    {
        var query = DbSet.Include(dp => dp.Orders)
            .Include(dp => dp.Shop)
            .ThenInclude(s => s.Account)
            .Include(dp => dp.ShopDeliveryStaff)
            .ThenInclude(sds => sds.Account)
            .Where(dp => dp.StartTime >= startTime
                         && dp.EndTime <= endTime
                         && dp.DeliveryDate.Date == deliveryDate.Date
                         && dp.Orders.All(o => o.ShopId == shopId))
            .AsQueryable();

        if (deliveryPackageId != default)
        {
            query = query.Where(dp => string.Concat("DP-", dp.Id).Contains(deliveryPackageId));
        }

        if (fullName != default)
        {
            query = query.Where(dp => dp.ShopId.HasValue && dp.Shop.Account.FullName.Contains(fullName) ||
                                      dp.ShopDeliveryStaffId.HasValue && dp.ShopDeliveryStaff.Account.FullName.Contains(fullName));
        }

        var total = query.Count();
        var results = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize).ToList();

        return (total, results);
    }

    public List<DeliveryPackage> GetAllDeliveryPackageInDate(DateTime deliveryDate, bool isShopOwnerShip, long shipperId, DeliveryPackageStatus[] status)
    {
        var result = DbSet.Where(dp => dp.DeliveryDate.Date == deliveryDate.Date
                                       && status.Contains(dp.Status)
                                       && (isShopOwnerShip && dp.ShopId.Value == shipperId ||
                                           !isShopOwnerShip && dp.ShopDeliveryStaffId.Value == shipperId)
        ).ToList();

        // Order by StartTime, then EndTime, then DeliveryDate
        return result.OrderBy(dp => dp.StartTime)
            .ThenBy(dp => dp.EndTime)
            .ThenBy(dp => dp.DeliveryDate)
            .ToList();
    }

    public List<DeliveryPackage> GetAllRequestUpdate(DateTime deliveryDate, int startTime, int endTime, List<long> staffIds)
    {
        var result = DbSet.Where(dp => dp.DeliveryDate.Date == deliveryDate.Date
                                       && dp.StartTime == startTime && dp.EndTime == endTime
                                       && (dp.ShopId.HasValue && staffIds.Contains(dp.ShopId.Value) ||
                                           dp.ShopDeliveryStaffId.HasValue && staffIds.Contains(dp.ShopDeliveryStaffId.Value))).ToList();

        return result;
    }

    public (int Total, List<DeliveryPackage> DeliveryPackages) GetAllOwnDeliveryPackageFilter(int pageIndex, int pageSize, DateTime deliveryDate, int startTime, int endTime, DeliveryPackageStatus[] status,
        string? requestDeliveryPackageId, long shopId)
    {
        var query = DbSet.Include(dp => dp.Orders)
            .Include(dp => dp.Shop)
            .ThenInclude(s => s.Account)
            .Include(dp => dp.ShopDeliveryStaff)
            .ThenInclude(sds => sds.Account)
            .Where(dp => dp.StartTime >= startTime
                         && dp.EndTime <= endTime
                         && dp.DeliveryDate.Date == deliveryDate.Date
                         && dp.Orders.All(o => o.ShopId == shopId)
                         && dp.ShopId == shopId
                         && status.Contains(dp.Status))
            .OrderBy(dp => dp.StartTime)
            .ThenBy(dp => dp.EndTime)
            .ThenBy(dp => dp.DeliveryDate)
            .AsQueryable();

        if (requestDeliveryPackageId != null)
        {
            query = query.Where(dp => string.Concat("DP-", dp.Id).Contains(requestDeliveryPackageId));
        }

        var total = query.Count();
        var results = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize).ToList();

        return (total, results);
    }

    public Task<bool> CheckHaveInDeliveryPackageNotDone(long shopDeliveryStaffId)
    {
        return DbSet
            .Where(dp => dp.ShopDeliveryStaffId == shopDeliveryStaffId && dp.Status != DeliveryPackageStatus.Done)
            .AnyAsync(dp => dp.Orders.Any(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Delivering));
    }

    public bool CheckIsExistDeliveryPackageBaseOnRole(bool isShopOwner, long deliveryPackageId, long? shipperId)
    {
        if (isShopOwner)
        {
            return DbSet.Any(dp => dp.Id == deliveryPackageId && (dp.ShopId == shipperId || dp.ShopDeliveryStaff.ShopId == shipperId));
        }

        return DbSet.Any(dp => dp.Id == deliveryPackageId && dp.ShopDeliveryStaffId == shipperId);
    }

    public DeliveryPackage GetByOrderId(long id)
    {
        return DbSet.Where(dp => dp.Orders.Any(o => o.Id == id)).FirstOrDefault();
    }
}