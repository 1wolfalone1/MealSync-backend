using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Shop GetShopConfiguration(long id);

    Task<Shop> GetByAccountId(long id);

    Task<int> CountTopShop(long dormitoryId);

    Task<IEnumerable<Shop>> GetTopShop(long dormitoryId, int pageIndex, int pageSize);

    Task<Shop?> GetShopInfoById(long id);
}