using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
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
}