using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopCategoryRepository : IBaseRepository<ShopCategory>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);

    ShopCategory? GetLastedByShopId(long shopId);

    ShopCategory? GetByIdAndShopId(long id, long shopId);

    List<ShopCategory> GetAllByShopId(long shopId);

    bool CheckExistName(string name, long? id = 0);
}