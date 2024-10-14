using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodOperatingSlotRepository : IBaseRepository<FoodOperatingSlot>
{
    List<FoodOperatingSlot> GetOperatingSlotsWithFoodByOpId(long operatingId);

    Task<bool> ExistedByFoodIdAndOperatingSlotId(long foodId, long operatingSlotId);
}