using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DormitoryDistanceRepository : BaseRepository<DormitoryDistance>, IDormitoryDistanceRepository
{

    public DormitoryDistanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public DormitoryDistance GetByIds(long dormitoryId1, long dormitoryId2)
    {
        return DbSet.Where(d => d.DormitoryFromId == dormitoryId1 && d.DormitoryToId == dormitoryId2 ||
                                d.DormitoryFromId == dormitoryId2 && d.DormitoryToId == dormitoryId1).SingleOrDefault();
    }
}