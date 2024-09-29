using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<bool> CheckExistedByIds(List<long> ids)
    {
        // Count the number of categories that have an ID in the given list
        var matchingCount = await DbSet.CountAsync(category => ids.Contains(category.Id));

        // Return true if the count of matching categories is the same as the count of IDs in the input list
        return matchingCount == ids.Count;
    }
}