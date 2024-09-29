using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Shop GetShopConfiguration(long id);

    Task<Shop> GetByAccountId(long id);
}