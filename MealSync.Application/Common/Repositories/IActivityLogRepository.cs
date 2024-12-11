using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IActivityLogRepository : IBaseRepository<ActivityLog>
{
    (int TotalCount, List<ActivityLog> Result) GetActivityLogPaging(string? searchValue, int actionType, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize);
}