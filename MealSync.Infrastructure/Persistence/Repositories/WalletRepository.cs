using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
{
    public WalletRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<Wallet> GetByType(WalletTypes type)
    {
        return DbSet.FirstAsync(w => w.Type == type);
    }
}