using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Food GetByIdIncludeAllInfo(long id);

    Task<(int TotalCount, IEnumerable<Food> Foods)> GetTopFood(long dormitoryId, int pageIndex, int pageSize);

    Task<List<(long CategoryId, IEnumerable<Food> Foods)>> GetShopFood(long shopId);

    Task<bool> CheckExistedAndActiveByIdAndShopId(long id, long shopId);
}