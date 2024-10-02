using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFavouriteRepository : IBaseRepository<Favourite>
{
    Favourite? GetByShopIdAndAccountId(long shopId, long accountId);
}