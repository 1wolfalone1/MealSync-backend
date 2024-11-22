using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    (int TotalCount, int TotalUnread, List<Notification> Notifications) GetListNotificationBaseOnAccountId(long accountId, int pageIndex, int pageSize);

    List<Notification> GetByIds(long accountId, long[] ids);

    List<Notification> GetNotificationUnRead(long accountId);
}