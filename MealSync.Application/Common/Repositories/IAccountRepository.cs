using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IAccountRepository : IBaseRepository<Account>
{
    Account? GetAccountByEmail(string email);

    bool CheckExistByPhoneNumber(string phoneNumber);
}
