using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOperatingSlotRepository : IBaseRepository<OperatingSlot>
{
    OperatingSlot? GetByIdAndShopId(long id, long shopId);
}