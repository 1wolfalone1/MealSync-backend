using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OperatingFrameRepository : BaseRepository<OperatingFrame>, IOperatingFrameRepository
{
    public OperatingFrameRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}