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

    public (int TotalCount, List<PlatformCategory> Result) GetPlatformCategoryForAdmin(string? searchValue, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.AsQueryable();

        if (searchValue != null)
        {
            query = query.Where(pl => pl.Id.ToString().Contains(searchValue)
                                      || pl.Name.Contains(searchValue));
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(pl => pl.CreatedDate.DateTime >= dateFrom && pl.CreatedDate.DateTime <= dateTo);
        }

        var totalCount = query.Count();
        var result = query.OrderByDescending(o => o.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, result);
    }
}