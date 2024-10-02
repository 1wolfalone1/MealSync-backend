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
            .SingleOrDefault(so => so.Id == id);
    }

    public async Task<Shop> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }

    public async Task<int> CountTopShop(long dormitoryId)
    {
        return await DbSet
            .CountAsync(
                shop => shop.ShopDormitories.Select(shopDormitory => shopDormitory.DormitoryId).Contains(dormitoryId)
                        && !shop.IsReceivingOrderPaused
                        && shop.Status == ShopStatus.Active
            ).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Shop>> GetTopShop(long dormitoryId, int pageIndex, int pageSize)
    {
        return await DbSet
            .Include(shop => shop.ShopDormitories)
            .Where(
                shop => shop.ShopDormitories.Select(shopDormitory => shopDormitory.DormitoryId).Contains(dormitoryId)
                        && !shop.IsReceivingOrderPaused
                        && shop.Status == ShopStatus.Active
            )
            .OrderByDescending(shop => (double)shop.TotalRating / shop.TotalReview) // High total rating / total review
            .ThenBy(shop => shop.NumOfWarning) // Low number of warnings
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}