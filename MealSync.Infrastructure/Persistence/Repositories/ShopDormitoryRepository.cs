using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopDormitoryRepository : BaseRepository<ShopDormitory>, IShopDormitoryRepository
{
    public ShopDormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> CheckExistedByShopIdAndDormitoryId(long shopId, long dormitoryId)
    {
        return DbSet.AnyAsync(sd => sd.ShopId == shopId && sd.DormitoryId == dormitoryId);
    }

    public Task<List<ShopDormitory>> GetByShopId(long shopId)
    {
        return DbSet.Where(sd => sd.ShopId == shopId).ToListAsync();
    }
}