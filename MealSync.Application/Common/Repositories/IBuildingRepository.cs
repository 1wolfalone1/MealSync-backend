using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IBuildingRepository : IBaseRepository<Building>
{
    Building GetBuildingById(long id);
}