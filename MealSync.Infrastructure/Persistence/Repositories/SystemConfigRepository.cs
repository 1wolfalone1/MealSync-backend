using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class SystemConfigRepository : BaseRepository<SystemConfig>, ISystemConfigRepository
{
    public SystemConfigRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}