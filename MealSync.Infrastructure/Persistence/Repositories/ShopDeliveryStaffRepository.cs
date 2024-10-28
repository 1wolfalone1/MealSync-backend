using Google.Rpc;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopDeliveryStaffRepository : BaseRepository<ShopDeliveryStaff>, IShopDeliveryStaffRepository
{
    public ShopDeliveryStaffRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public List<ShopDeliveryStaff> GetListAvailableShopDeliveryStaff(string? searchText, int orderMode, long shopId)
    {
        var query = DbSet.Include(sdf => sdf.Account)
            .Include(sdf => sdf.DeliveryPackages.Where(dp => (dp.Status == DeliveryPackageStatus.Created || dp.Status == DeliveryPackageStatus.OnGoing)
                                                             && DateTime.Compare(dp.DeliveryDate.Date, TimeFrameUtils.GetCurrentDateInUTC7().Date) == 0))
            .Where(sdf => sdf.ShopId == shopId && sdf.Status != ShopDeliveryStaffStatus.Offline).AsQueryable();

        if (searchText != default)
        {
            query = query.Where(sdf => sdf.Account.FullName.Contains(searchText) || sdf.Account.PhoneNumber.Contains(searchText));
        }

        // order by mode = 0 ASC status
        if (orderMode == 0)
        {
            query = query.OrderBy(sdf => sdf.Status);
        }

        // order by mode = 1 DESC status
        if (orderMode == 1)
        {
            query = query.OrderByDescending(sdf => sdf.Status);
        }

        return query.ToList();
    }
}