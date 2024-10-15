using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopDormitoryRepository : IBaseRepository<ShopDormitory>
{
    Task<bool> CheckExistedByShopIdAndDormitoryId(long shopId, long dormitoryId);
}