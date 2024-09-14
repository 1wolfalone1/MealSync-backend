using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WalletTransactionRepository : BaseRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}