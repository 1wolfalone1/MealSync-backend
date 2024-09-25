using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOperatingDayRepository : IBaseRepository<OperatingDay>
{
    OperatingDay? GetByIdAndShopId(long id, long shopId);
}