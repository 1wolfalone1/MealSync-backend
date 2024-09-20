using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IAccountRepository : IBaseRepository<Account>
{
    Account GetCustomerAccount(string email, string password);

    Account? GetAccountByEmail(string email);
}
