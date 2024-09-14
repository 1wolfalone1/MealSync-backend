using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ModeratorActivityLogRepository : BaseRepository<ModeratorActivityLog>, IModeratorActivityLogRepository
{
    public ModeratorActivityLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}