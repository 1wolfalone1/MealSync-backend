using MealSync.Application.UseCases.Shops.Queries.SearchShop;
using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Shop GetShopConfiguration(long id);

    Task<Shop> GetByAccountId(long id);

    Task<Shop?> GetShopInfoByIdForCustomer(long id);

    Task<(int TotalCount, IEnumerable<Shop> Shops)> GetTopShop(long dormitoryId, int pageIndex, int pageSize);

    Task<Shop?> GetByIdIncludeLocation(long id);

    Task<(int TotalCounts, List<Shop> Shops)> SearchShops(
        long dormitoryId, string? searchValue, int? platformCategoryId,
        int? startTime, int? endTime, int foodSize,
        SearchShopQuery.OrderBy? orderBy, SearchShopQuery.Direction direction,
        int pageIndex, int pageSize);

    Task<Shop> GetShopInfoByIdForShop(long id);

    Task<Shop> GetShopInfoForReOrderById(long id);

    Task<List<Shop>> GetAllShopReceivingOrderPaused();
}