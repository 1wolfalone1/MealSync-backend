using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IWalletRepository : IBaseRepository<Wallet>
{
    Task<Wallet> GetByType(WalletTypes type);

    Task<Wallet> GetIncludeWithdrawalRequest(long id);
}