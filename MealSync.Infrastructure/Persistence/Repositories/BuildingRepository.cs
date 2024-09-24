using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class BuildingRepository : BaseRepository<Building>, IBuildingRepository
{
    public BuildingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Building GetById(long id)
    {
        return DbSet.Include(b => b.Dormitory)
            .ThenInclude(d => d.Location)
            .Include(b => b.Location)
            .Single(b => b.Id == id);
    }

    public List<Building> GetByDormitoryIdAndName(long dormitoryId, string name)
    {
        return DbSet.Include(b => b.Dormitory)
            .ThenInclude(d => d.Location)
            .Include(b => b.Location)
            .Where(b => b.DormitoryId == dormitoryId && b.Name.Contains(name)).ToList();
    }
}