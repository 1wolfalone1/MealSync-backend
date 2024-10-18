using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public ShopCategory? GetByIdAndShopId(long id, long shopId)
    {
        return DbSet.FirstOrDefault(sc => sc.Id == id && sc.ShopId == shopId);
    }

    public List<ShopCategory> GetAllByShopId(long shopId)
    {
        return DbSet.Where(sc => sc.ShopId == shopId).OrderBy(sc => sc.DisplayOrder).ToList();
    }

    public bool CheckExistName(string name, long? id)
    {
        return id == null
            ? DbSet.Any(sc => sc.Name == name)
            : DbSet.Any(sc => sc.Name == name && sc.Id != id);
    }
}