using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WithdrawalRequestRepository : BaseRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}