using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Food GetByIdIncludeAllInfoForCustomer(long id);

    Food GetByIdIncludeAllInfoForShop(long id);

    Task<(int TotalCount, IEnumerable<Food> Foods)> GetTopFood(long dormitoryId, int pageIndex, int pageSize);

    Task<bool> CheckExistedAndActiveByIdAndShopId(long id, long shopId);

    Task<List<(long CategoryId, string CategoryName, IEnumerable<Food> Foods)>> GetShopFood(long shopId);

    Task<bool> CheckExistedByIdAndShopId(long id, long shopId);

    Task<List<(long CategoryId, string CategoryName, IEnumerable<Food> Foods)>> GetShopOwnerFood(long shopId);

    Task<(int TotalCount, IEnumerable<Food> Foods)> GetAllActiveFoodByShopId(long shopId, int pageIndex, int pageSize);

    Task<(List<long> IdsNotFound, IEnumerable<Food> Foods)> GetByIds(List<long> ids);

    Task<bool> CheckAllIdsInOneShop(List<long> ids);

    Task<Food?> GetByIdAndShopId(long id, long shopId);
}