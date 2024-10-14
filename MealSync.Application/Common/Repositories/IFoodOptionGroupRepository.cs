using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodOptionGroupRepository : IBaseRepository<FoodOptionGroup>
{
    int GetMaxCurrentDisplayOrder(long foodId);

    Task<FoodOptionGroup?> GetActiveOptionGroupByFoodIdAndOptionGroupId(long foodId, long optionGroupId);

    Task<List<long>> GetAllIdsRequiredByFoodId(long foodId);
}