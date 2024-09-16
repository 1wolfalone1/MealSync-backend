using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
{
    public WalletRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}