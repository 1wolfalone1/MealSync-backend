using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IWalletTransactionRepository : IBaseRepository<WalletTransaction>
{
    (int TotalCount, List<WalletTransaction> WalletTransactions) GetShopTransactionHistory(long shopWalletId, int pageIndex, int pageSize);
}