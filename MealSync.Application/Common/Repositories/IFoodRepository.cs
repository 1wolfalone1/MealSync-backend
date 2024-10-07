using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Food GetByIdIncludeAllInfo(long id);

    Task<(int TotalCount, IEnumerable<Food> Foods)> GetTopFood(long dormitoryId, int pageIndex, int pageSize);

    Task<List<(long CategoryId, string CategoryName, IEnumerable<Food> Foods)>> GetShopFood(long shopId);
}