using MealSync.Application.Common.Repositories;
using MealSync.Batch.Services;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class DailyBatchCheckerJob : IJob
{
    private readonly BatchCheckDatabaseService _databaseService;

    public DailyBatchCheckerJob(BatchCheckDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _databaseService.CheckAndRunPendingBatches().ConfigureAwait(false);
    }
}