using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopOwnerRepository : BaseRepository<ShopOwner>, IShopOwnerRepository
{
    public ShopOwnerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}