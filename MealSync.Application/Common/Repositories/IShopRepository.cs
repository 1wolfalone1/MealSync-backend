using MealSync.Application.UseCases.Shops.Models;
using MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetListShop;
using MealSync.Application.UseCases.Shops.Queries.SearchShop;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

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

    Task<(List<ShopManageDto> Shops, int TotalCount)> GetAllShopByDormitoryIds(List<long> dormitoryIds, string? searchValue, DateTime? dateFrom, DateTime? dateTo, ShopStatus? status, long? dormitoryId,
        GetListShopQuery.FilterShopOrderBy? orderBy, GetListShopQuery.FilterShopDirection? direction, int pageIndex, int pageSize);

    Task<Shop?> GetShopManageDetail(long shopId, List<long> dormitoriesIdMod);

    Task<Shop?> GetShopManage(long shopId, List<long> dormitoriesIdMod);

    Task<double> GetShopRevenue(long shopId);

    Task<List<Shop>> GetShopByIds(List<long> ids);
}