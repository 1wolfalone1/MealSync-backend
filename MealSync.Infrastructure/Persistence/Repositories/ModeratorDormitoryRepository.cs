using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ModeratorDormitoryRepository : BaseRepository<ModeratorDormitory>, IModeratorDormitoryRepository
{
    public ModeratorDormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}