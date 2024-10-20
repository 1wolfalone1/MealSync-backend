using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopCategoryRepository : IBaseRepository<ShopCategory>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);

    ShopCategory? GetLastedByShopId(long shopId);

    ShopCategory? GetByIdAndShopId(long id, long shopId);

    List<ShopCategory> GetAllByShopId(long shopId);

    bool CheckExistName(string name, long shopId, long? id = 0);

    Task<(int TotalCount, IEnumerable<ShopCategory> ShopCategories)> GetAllShopCategoriesAsync(int pageIndex, int pageSize, string? name);
}