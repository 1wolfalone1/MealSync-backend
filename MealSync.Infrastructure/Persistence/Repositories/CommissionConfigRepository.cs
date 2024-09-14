using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CommissionConfigRepository : BaseRepository<CommissionConfig>, ICommissionConfigRepository
{
    public CommissionConfigRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}