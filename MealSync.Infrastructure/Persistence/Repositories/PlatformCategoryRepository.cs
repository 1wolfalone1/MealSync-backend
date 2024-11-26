using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PlatformCategoryRepository : BaseRepository<PlatformCategory>, IPlatformCategoryRepository
{
    public PlatformCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedById(long id)
    {
        return DbSet.Any(pc => pc.Id == id);
    }

    public async Task<IEnumerable<PlatformCategory>> GetAll()
    {
        return await DbSet.OrderBy(p => p.DisplayOrder).ToListAsync().ConfigureAwait(false);
    }

    public bool CheckExsitName(string name)
    {
        return DbSet.Any(pl => pl.Name.ToLower() == name.ToLower());
    }

    public int GetMaxDisplayOrder()
    {
        return DbSet.Max(pl => pl.DisplayOrder);
    }

    public bool CheckExsitUpdateName(string requestName, long requestId)
    {
        return DbSet.Any(pl => pl.Name.ToLower() == requestName.ToLower() && pl.Id != requestId);
    }

    public List<PlatformCategory> GetByIds(long[] ids)
    {
        var platform = new List<PlatformCategory>();
        foreach (var id in ids)
        {
            platform.Add(DbSet.Where(pl => pl.Id == id).Single());
        }

        return platform;
    }
}