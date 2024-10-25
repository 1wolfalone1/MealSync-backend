using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IAccountRepository : IBaseRepository<Account>
{
    Account? GetAccountByEmail(string email);

    Account GetAccountByPhoneNumber(string registerPhoneNumber);

    bool CheckExistByPhoneNumber(string phoneNumber);

    Account? GetCustomerById(long id);

    List<Account>? GetAccountsOfModeratorByDormitoryId(long dormitoryId);
}