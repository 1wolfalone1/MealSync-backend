using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Services;

public class ActivityLogService : BaseService, IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivityLogService(IUnitOfWork unitOfWork, IActivityLogRepository activityLogRepository)
    {
        _unitOfWork = unitOfWork;
        _activityLogRepository = activityLogRepository;
    }

    public async Task LogActivity(ActivityLog log)
    {
        try
        {
            await this._unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await this._activityLogRepository.AddAsync(log).ConfigureAwait(false);
            await this._unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch
        {
            this._unitOfWork.RollbackTransaction();
            throw;
        }
    }
}