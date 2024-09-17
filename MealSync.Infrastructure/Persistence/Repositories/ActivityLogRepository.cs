using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}