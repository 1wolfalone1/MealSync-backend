using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DepositRepository : BaseRepository<Deposit>, IDepositRepository
{
    public DepositRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}