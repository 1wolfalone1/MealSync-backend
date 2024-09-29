using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopRepository : BaseRepository<Shop>, IShopRepository
{
    public ShopRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Shop GetShopConfiguration(long id)
    {
        return this.DbSet.Include(so => so.Location)
            .Include(so => so.OperatingSlots)
            .Include(so => so.Location)
            .Include(so => so.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .SingleOrDefault(so => so.Id == id);
    }

    public async Task<Shop> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }
}