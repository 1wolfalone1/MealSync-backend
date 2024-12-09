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
            .Include(sdf => sdf.DeliveryPackages.Where(dp => (dp.Status == DeliveryPackageStatus.InProcess)
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

    public async Task<(int TotalCounts, List<ShopDeliveryStaff> ShopDeliveryStaffs)> GetAllStaffOfShop(
        long shopId, string? searchValue, ShopDeliveryStaffStatus? status, int pageIndex, int pageSize)
    {
        var query = DbSet.Include(sds => sds.Account)
            .Where(sds => sds.ShopId == shopId && sds.Account.Status != AccountStatus.Deleted).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(sds => sds.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            query = query.Where(sds =>
                EF.Functions.Like(sds.Account.FullName ?? string.Empty, $"%{searchValue}%") ||
                EF.Functions.Like(sds.Account.Email, $"%{searchValue}%") ||
                EF.Functions.Like(sds.Account.PhoneNumber, $"%{searchValue}%")
            );
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);
        query = query.OrderByDescending(sds => sds.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        var shopDeliveryStaffs = await query.ToListAsync().ConfigureAwait(false);

        return (totalCount, shopDeliveryStaffs);
    }

    public Task<ShopDeliveryStaff?> GetByIdAndShopId(long id, long shopId)
    {
        return DbSet.Include(sds => sds.Account)
            .FirstOrDefaultAsync(sds => sds.Id == id && sds.ShopId == shopId && sds.Account.Status != AccountStatus.Deleted);
    }

    public Task<ShopDeliveryStaff> GetByIdIncludeAccount(long id)
    {
        return DbSet.Include(sds => sds.Account).FirstAsync(sds => sds.Id == id);
    }

    public bool CheckStaffOfShopActiveAndStaffActive(long id)
    {
        return DbSet.Any(sds => sds.Id == id && sds.Status != ShopDeliveryStaffStatus.InActive && (sds.Shop.Status == ShopStatus.Active || sds.Shop.Status == ShopStatus.InActive));
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}