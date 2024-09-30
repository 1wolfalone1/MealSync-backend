using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodRepository : BaseRepository<Food>, IFoodRepository
{
    public FoodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Food GetByIdIncludeAllInfo(long id)
    {
        return DbSet.Include(p => p.PlatformCategory)
            .First(p => p.Id == id);
    }
}