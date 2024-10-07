using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFavouriteRepository : IBaseRepository<Favourite>
{
    Favourite? GetByShopIdAndAccountId(long shopId, long accountId);

    Task<(int TotalCount, IEnumerable<Favourite> Favourites)> GetAllByAccountId(long accountId, int pageIndex, int pageSize);
}