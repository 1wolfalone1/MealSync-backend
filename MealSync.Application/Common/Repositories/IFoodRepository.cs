using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Food GetByIdIncludeAllInfo(long id);

    Task<int> CountTopFood(long dormitoryId);

    Task<IEnumerable<Food>> GetTopFood(long dormitoryId, int pageIndex, int pageSize);
}