using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CommissionConfigRepository : BaseRepository<CommissionConfig>, ICommissionConfigRepository
{
    public CommissionConfigRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public double GetCommissionConfig()
    {
        return DbSet.OrderByDescending(cc => cc.CreatedDate).Select(cc => cc.CommissionRate).First();
    }

    public CommissionConfig GetNewest()
    {
        return DbSet.OrderByDescending(cc => cc.CreatedDate).First();
    }
}