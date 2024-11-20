using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;


namespace MealSync.Infrastructure.Persistence.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, int TotalUnread, List<Notification> Notifications) GetListNotificationBaseOnAccountId(long accountId, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(no => no.AccountId == accountId);
        var totalCount = query.Count();
        var totalUnRead = query.Where(n => !n.IsRead).Count();
        var result = query
            .OrderByDescending(n => n.CreatedBy).Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return (totalCount, totalUnRead, result);
    }

    public List<Notification> GetByIds(long accountId, long[] ids)
    {
        return DbSet.Where(n => n.AccountId == accountId && ids.Contains(n.Id)).ToList();
    }
}