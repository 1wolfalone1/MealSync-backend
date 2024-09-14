using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DormitoryRepository : BaseRepository<Dormitory>, IDormitoryRepository
{
    public DormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}