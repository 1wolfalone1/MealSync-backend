using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface ILocationRepository : IBaseRepository<Location>
{
    Location GetById(long id);
}