using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IBuildingRepository : IBaseRepository<Building>
{
    List<Building> GetByDormitoryIdAndName(long dormitoryId, string name);

    Task<Building?> GetByIdIncludeLocation(long id);
}