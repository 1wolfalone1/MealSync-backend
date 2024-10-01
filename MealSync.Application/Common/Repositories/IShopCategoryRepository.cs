using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopCategoryRepository : IBaseRepository<ShopCategory>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);

    ShopCategory? GetLastedByShopId(long shopId);
}