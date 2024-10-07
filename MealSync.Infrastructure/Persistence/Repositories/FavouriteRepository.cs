using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FavouriteRepository : BaseRepository<Favourite>, IFavouriteRepository
{
    public FavouriteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Favourite? GetByShopIdAndAccountId(long shopId, long accountId)
    {
        return DbSet.FirstOrDefault(f => f.ShopId == shopId && f.CustomerId == accountId);
    }

    public async Task<(int TotalCount, IEnumerable<Favourite> Favourites)> GetAllByAccountId(long accountId, int pageIndex, int pageSize)
    {
        var query = DbSet.Include(f => f.Shop)
            .Where(
                f => f.CustomerId == accountId
                     && (f.Shop.Status == ShopStatus.Active || f.Shop.Status == ShopStatus.InActive)
            ).AsQueryable();
        var totalCount = await query.CountAsync().ConfigureAwait(false);
        var favourites = await query
            .OrderByDescending(f => f.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync().ConfigureAwait(false);

        return (totalCount, favourites);
    }
}