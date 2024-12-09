using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
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

    public Account GetAccountByPhoneNumber(string registerPhoneNumber)
    {
        return this.DbSet.SingleOrDefault(a => a.PhoneNumber == registerPhoneNumber);
    }

    public bool CheckExistByPhoneNumber(string phoneNumber)
    {
        return DbSet.Any(a => a.PhoneNumber == phoneNumber);
    }

    public Account? GetCustomerById(long id)
    {
        return DbSet.Include(a => a.Customer)
            .FirstOrDefault(a => a.Id == id && a.RoleId == (int)Roles.Customer && a.Status == AccountStatus.Verify);
    }

    public List<Account>? GetAccountsOfModeratorByDormitoryId(long dormitoryId)
    {
        return DbSet.Include(a => a.Moderator)
            .ThenInclude(m => m.ModeratorDormitories)
            .Where(a => a.RoleId == (int)Roles.Moderator &&
                        a.Moderator.ModeratorDormitories.Any(md => md.DormitoryId == dormitoryId)).ToList();
    }

    public Account GetIncludeCustomerById(long id)
    {
        return DbSet.Include(a => a.Customer).First(a => a.Id == id);
    }

    public bool CheckExistPhoneNumberInOtherEmailAccount(string email, string phoneNumber)
    {
        return DbSet.Any(a => a.PhoneNumber == phoneNumber && a.Email != email);
    }

    public List<Account> GetAccountByIds(List<long> ids)
    {
        return DbSet.Where(a => ids.Contains(a.Id)).ToList();
    }
}