using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WalletHistoryRepository : BaseRepository<WalletHistory>, IWalletHistoryRepository
{
    public WalletHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}