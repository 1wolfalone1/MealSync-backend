using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodOptionGroupRepository : BaseRepository<FoodOptionGroup>, IFoodOptionGroupRepository
{

    public FoodOptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public int GetMaxCurrentDisplayOrder(long foodId)
    {
        var list = DbSet.Where(fog => fog.FoodId == foodId).ToList();

        if (list != null && list.Count > 0)
            return list.Max(fog => fog.DisplayOrder);

        return 0;
    }

    public Task<FoodOptionGroup?> GetActiveOptionGroupByFoodIdAndOptionGroupId(long foodId, long optionGroupId)
    {
        return DbSet.Include(fog => fog.OptionGroup)
            .FirstOrDefaultAsync(fog =>
                fog.FoodId == foodId
                && fog.OptionGroupId == optionGroupId
                && fog.OptionGroup.Status == OptionGroupStatus.Active);
    }

    public Task<List<long>> GetAllIdsRequiredByFoodId(long foodId)
    {
        return DbSet.Where(fog => fog.FoodId == foodId && fog.OptionGroup.IsRequire && fog.OptionGroup.Status == OptionGroupStatus.Active)
            .Select(fog => fog.OptionGroupId).ToListAsync();
    }
}