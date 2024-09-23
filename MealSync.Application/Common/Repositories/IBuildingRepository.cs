using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IBuildingRepository : IBaseRepository<Building>
{
    Building GetById(long id);

    List<Building> GetByDormitoryIdAndName(long dormitoryId, string name);
}