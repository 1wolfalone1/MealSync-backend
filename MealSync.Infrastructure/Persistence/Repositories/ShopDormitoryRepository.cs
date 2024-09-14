using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopDormitoryRepository : BaseRepository<ShopDormitory>, IShopDormitoryRepository
{
    public ShopDormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}