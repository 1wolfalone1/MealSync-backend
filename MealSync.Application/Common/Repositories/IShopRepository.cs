using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Shop GetShopConfiguration(long id);

    Task<Shop> GetByAccountId(long id);

    Task<Shop?> GetShopInfoById(long id);

    Task<(int TotalCount, IEnumerable<Shop> Shops)> GetTopShop(long dormitoryId, int pageIndex, int pageSize);
}