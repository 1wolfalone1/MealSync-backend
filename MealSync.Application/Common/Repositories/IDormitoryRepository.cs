using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IDormitoryRepository : IBaseRepository<Dormitory>
{
    List<Dormitory> GetAll();
}