using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

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
}