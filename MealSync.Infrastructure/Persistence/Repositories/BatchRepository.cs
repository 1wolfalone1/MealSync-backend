using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class BatchRepository : BaseRepository<Batch>, IBatchRepository
{

    public BatchRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}