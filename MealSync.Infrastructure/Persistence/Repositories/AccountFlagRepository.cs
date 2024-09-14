using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class AccountFlagRepository : BaseRepository<AccountFlag>, IAccountFlagRepository
{
    public AccountFlagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}