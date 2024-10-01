using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopCategoryRepository : BaseRepository<ShopCategory>, IShopCategoryRepository
{
    public ShopCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedByIdAndShopId(long id, long shopId)
    {
        return DbSet.Any(s => s.Id == id && s.ShopId == shopId);
    }

    public ShopCategory? GetLastedByShopId(long shopId)
    {
        return DbSet.Where(sc => sc.ShopId == shopId).OrderByDescending(sc => sc.DisplayOrder).FirstOrDefault();
    }
}