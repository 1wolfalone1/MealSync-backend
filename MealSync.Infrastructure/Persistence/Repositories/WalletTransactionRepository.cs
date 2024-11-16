using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WalletTransactionRepository : BaseRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, List<WalletTransaction> WalletTransactions) GetShopTransactionHistory(long shopWalletId, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(wt => wt.WalletFromId == shopWalletId && wt.Type == WalletTransactionType.Withdrawal || wt.WalletToId == shopWalletId && wt.Type == WalletTransactionType.Transfer).AsQueryable();
        var totalCount = query.Count();
        var resultList = query.Skip((pageIndex - 1) * pageSize)
            .OrderByDescending(wlt => wlt.CreatedDate)
            .Take(pageSize).ToList();
        return (totalCount, resultList);
    }
}