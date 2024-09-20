using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services;

public interface IActivityLogService : IBaseService
{
    Task LogActivity(ActivityLog log);
}