using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopOwnerRepository : IBaseRepository<ShopOwner>
{
    Task<ShopOwner> GetByAccountId(long id);
}