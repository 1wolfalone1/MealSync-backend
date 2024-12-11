using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, List<ActivityLog> Result) GetActivityLogPaging(string? searchValue, int actionType, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.Include(al => al.Account).Where(al => al.TargetType != 0).AsQueryable();

        if (searchValue != default)
        {
            query = query.Where(ac => ac.Id.ToString().Contains(searchValue) ||
                                      ac.Account.Id.ToString().Contains(searchValue) ||
                                      ac.Account.FullName.Contains(searchValue)
                                      || ac.Account.Email.Contains(searchValue));
        }

        if (actionType != 0)
        {
            query = query.Where(al => (int)al.TargetType == actionType);
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(al => al.CreatedDate.DateTime >= dateFrom && al.CreatedDate.DateTime <= dateTo);
        }

        var totalCount = query.Count();
        var result = query
        .OrderByDescending(al => al.CreatedDate)
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToList();

        return (totalCount, result);
    }
}