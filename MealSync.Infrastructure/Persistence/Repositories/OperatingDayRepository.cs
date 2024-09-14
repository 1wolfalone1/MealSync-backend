using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OperatingDayRepository : BaseRepository<OperatingDay>, IOperatingDayRepository
{
    public OperatingDayRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}