using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopOwnerRepository : IBaseRepository<ShopOwner>
{
    ShopOwner GetShopConfiguration(long id);

    Task<ShopOwner> GetByAccountId(long id);
}