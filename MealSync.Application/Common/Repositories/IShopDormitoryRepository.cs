using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopDormitoryRepository : IBaseRepository<ShopDormitory>
{
    Task<bool> CheckExistedByShopIdAndDormitoryId(long shopId, long dormitoryId);

    Task<List<ShopDormitory>> GetByShopId(long shopId);

    Task<bool> CheckShopDormitory(long shopId, List<long> dormitoryIds);
}