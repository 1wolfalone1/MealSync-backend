using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class LocationRepository : BaseRepository<Location>, ILocationRepository
{
    public LocationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Location GetById(long id)
    {
        return DbSet.Single(l => l.Id == id);
    }
}