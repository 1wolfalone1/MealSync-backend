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
        return DbSet.Include(f => f.PlatformCategory)
            .Include(f => f.ShopCategory)
            .Include(f => f.FoodOperatingSlots).ThenInclude(op => op.OperatingSlot)
            .Include(f => f.FoodOptionGroups).ThenInclude(fog => fog.OptionGroup).ThenInclude(og => og.Options)
            .First(f => f.Id == id);
    }
}