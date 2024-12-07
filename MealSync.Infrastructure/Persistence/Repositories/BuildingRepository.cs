using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class BuildingRepository : BaseRepository<Building>, IBuildingRepository
{
    public BuildingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public List<Building> GetByDormitoryIdAndName(long dormitoryId, string name)
    {
        return DbSet.Include(b => b.Dormitory)
            .ThenInclude(d => d.Location)
            .Include(b => b.Location)
            .Where(b => b.DormitoryId == dormitoryId && b.Name.Contains(name)).ToList();
    }

    public Task<Building?> GetByIdIncludeLocation(long id)
    {
        return DbSet.Include(b => b.Location).Include(b => b.Dormitory).FirstOrDefaultAsync(b => b.Id == id);
    }

    public List<Location> GetListLocationBaseOnBuildingIds(List<long> ids)
    {
        return DbSet.Where(b => ids.Contains(b.Id)).Select(b => b.Location).ToList();
    }
}