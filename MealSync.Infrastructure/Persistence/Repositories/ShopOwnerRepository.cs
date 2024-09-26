using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopOwnerRepository : BaseRepository<ShopOwner>, IShopOwnerRepository
{
    public ShopOwnerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public ShopOwner GetShopConfiguration(long id)
    {
        return this.DbSet.Include(so => so.Location)
            .Include(so => so.OperatingDays)
            .ThenInclude(od => od.OperatingFrames)
            .Include(so => so.Location)
            .Include(so => so.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .SingleOrDefault(so => so.Id == id);
    }

    public async Task<ShopOwner> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }
}