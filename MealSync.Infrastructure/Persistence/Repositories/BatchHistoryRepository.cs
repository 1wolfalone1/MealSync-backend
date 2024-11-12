using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class BatchHistoryRepository : BaseRepository<BatchHistory>, IBatchHistoryRepository
{

    public BatchHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}