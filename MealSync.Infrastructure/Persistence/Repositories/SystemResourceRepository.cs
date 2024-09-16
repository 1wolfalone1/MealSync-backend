using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class SystemResourceRepository : BaseRepository<SystemResource>, ISystemResourceRepository
{
    public SystemResourceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}