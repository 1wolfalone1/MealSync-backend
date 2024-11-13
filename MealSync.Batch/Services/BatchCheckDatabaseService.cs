using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Batch.BatchLogic;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Batch.Services;

public class BatchCheckDatabaseService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchCheckDatabaseService> _logger;

    public BatchCheckDatabaseService(IServiceProvider serviceProvider, IBatchRepository batchRepository, IUnitOfWork unitOfWork, ILogger<BatchCheckDatabaseService> logger)
    {
        _serviceProvider = serviceProvider;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CheckAndRunPendingBatches()
    {
        // Assume you fetch pending batches with their BatchCode from the database
        _logger.LogInformation($"[START SCAN BATCH AT: {TimeFrameUtils.GetCurrentDateInUTC7()}");
        var pendingBatches = await FetchPendingBatchesAsync().ConfigureAwait(false);
        _logger.LogInformation($"Have {pendingBatches.Count} batch");

        foreach (var batch in pendingBatches)
        {
            var batchCodeName = batch.BatchCode;
            var batchService = ResolveBatchService(batchCodeName.GetDescription());

            if (batchService != null)
            {
                await batchService.ExecuteAsync();
                UpdateBatchStatusAsync(batch.Id, BatchStatus.Completed); // Mark batch as completed
            }
            else
            {
                Console.WriteLine($"No batch service found for {batchCodeName}");
                UpdateBatchStatusAsync(batch.Id, BatchStatus.Failed); // Mark batch as failed
            }
        }

        _logger.LogInformation($"[END SCAN BATCH AT: {TimeFrameUtils.GetCurrentDateInUTC7()}");
    }

    private IBatchService ResolveBatchService(string batchCodeName)
    {
        // Find the batch service by name
        batchCodeName = "MealSync.Batch.BatchLogic." + batchCodeName;
        var batchServiceType = Type.GetType(batchCodeName);
        
        if (batchServiceType != null && typeof(IBatchService).IsAssignableFrom(batchServiceType))
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, batchServiceType) as IBatchService;
        }
        
        return null;
    }

    private async Task UpdateBatchStatusAsync(long batchId, BatchStatus status)
    {
        // Implementation to update the batch status in the database
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batch = _batchRepository.GetById(batchId);
            batch.Status = status;
            _batchRepository.Update(batch);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private async Task<List<Domain.Entities.Batch>> FetchPendingBatchesAsync()
    {
        // Implementation to fetch pending batches from the database
        var batchs = await _batchRepository.Get(b => b.Status == BatchStatus.Pending && b.ScheduledTime >= TimeFrameUtils.GetCurrentDate()).ToListAsync().ConfigureAwait(false);
        return batchs;
    }
}