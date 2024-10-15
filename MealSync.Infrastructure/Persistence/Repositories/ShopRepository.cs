using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopRepository : BaseRepository<Shop>, IShopRepository
{
    public ShopRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Shop GetShopConfiguration(long id)
    {
        return this.DbSet.Include(so => so.Location)
            .Include(so => so.OperatingSlots)
            .Include(so => so.Location)
            .Include(so => so.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .Include(so => so.Account)
            .SingleOrDefault(so => so.Id == id);
    }

    public async Task<Shop> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }

    public async Task<(int TotalCount, IEnumerable<Shop> Shops)> GetTopShop(long dormitoryId, int pageIndex, int pageSize)
    {
        // Query to get a paginated list of shops associated with the given dormitory ID
        // The shop must be active and not paused for receiving orders
        var query = DbSet
            .Include(shop => shop.ShopDormitories)
            .Where(
                shop => shop.ShopDormitories.Select(shopDormitory => shopDormitory.DormitoryId).Contains(dormitoryId)
                        && !shop.IsReceivingOrderPaused
                        && shop.Status == ShopStatus.Active
            );
        var totalCount = await query.CountAsync().ConfigureAwait(false);
        var shops = await query
            .OrderByDescending(shop => (double)shop.TotalRating / shop.TotalReview) // High total rating / total review
            .ThenBy(shop => shop.NumOfWarning) // Low number of warnings
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        return (totalCount, shops);
    }

    public Task<Shop?> GetByIdIncludeLocation(long id)
    {
        return DbSet.Include(s => s.Location).FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shop?> GetShopInfoById(long id)
    {
        return await DbSet.Include(s => s.OperatingSlots)
            .Include(s => s.Location)
            .Include(s => s.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .FirstOrDefaultAsync(s => s.Id == id && s.Status != ShopStatus.Deleted && s.Status != ShopStatus.UnApprove).ConfigureAwait(false);
    }
}