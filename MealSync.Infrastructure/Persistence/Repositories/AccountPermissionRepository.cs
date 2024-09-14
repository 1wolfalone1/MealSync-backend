using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class AccountPermissionRepository : BaseRepository<AccountPermission>, IAccountPermissionRepository
{
    public AccountPermissionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}