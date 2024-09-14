using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Account GetCustomerAccount(string email, string password)
    {
        return this.DbSet.SingleOrDefault(a => a.Email == email 
                                               && password == password);
    }
}

