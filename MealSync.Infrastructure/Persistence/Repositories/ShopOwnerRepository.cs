using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopOwnerRepository : BaseRepository<ShopOwner>, IShopOwnerRepository
{
    public ShopOwnerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<ShopOwner> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }
}