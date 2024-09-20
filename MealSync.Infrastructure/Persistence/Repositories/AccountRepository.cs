using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Account? GetAccountByEmail(string email)
    {
        return DbSet.Include(a => a.Role).SingleOrDefault(a => a.Email == email);
    }
}

