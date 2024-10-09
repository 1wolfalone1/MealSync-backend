using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodOptionGroupRepository : IBaseRepository<FoodOptionGroup>
{
    int GetMaxCurrentDisplayOrder(long foodId);
}