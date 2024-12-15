using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IDormitoryDistanceRepository : IBaseRepository<DormitoryDistance>
{
    DormitoryDistance GetByIds(long dormitoryId1, long dormitoryId2);
}