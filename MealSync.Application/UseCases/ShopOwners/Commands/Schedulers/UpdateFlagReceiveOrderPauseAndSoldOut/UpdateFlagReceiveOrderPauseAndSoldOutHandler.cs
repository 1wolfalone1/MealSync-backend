using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.Schedulers.UpdateFlagReceiveOrderPauseAndSoldOut;

public class UpdateFlagReceiveOrderPauseAndSoldOutHandler : ICommandHandler<UpdateFlagReceiveOrderPauseAndSoldOutCommand, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBatchHistoryRepository _batchHistoryRepository;
    private readonly ILogger<UpdateFlagReceiveOrderPauseAndSoldOutHandler> _logger;

    public UpdateFlagReceiveOrderPauseAndSoldOutHandler(
        IShopRepository shopRepository, IOperatingSlotRepository operatingSlotRepository,
        IFoodRepository foodRepository, IUnitOfWork unitOfWork, IBatchHistoryRepository batchHistoryRepository,
        ILogger<UpdateFlagReceiveOrderPauseAndSoldOutHandler> logger)
    {
        _shopRepository = shopRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _foodRepository = foodRepository;
        _unitOfWork = unitOfWork;
        _batchHistoryRepository = batchHistoryRepository;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateFlagReceiveOrderPauseAndSoldOutCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var startBatchDateTime = TimeFrameUtils.GetCurrentDate();
        var endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        _logger.LogInformation($"Update Flag Receive Order Pause And Sold Out Batch Start At: {startBatchDateTime}");

        var shopReceivingOrderPaused = await _shopRepository.GetAllShopReceivingOrderPaused().ConfigureAwait(false);
        var slotReceivingOrderPaused = await _operatingSlotRepository.GetAllSlotReceivingOrderPaused().ConfigureAwait(false);
        var foodIsSoldOut = await _foodRepository.GetAllFoodIsSoldOut().ConfigureAwait(false);

        shopReceivingOrderPaused.ForEach(s => s.IsReceivingOrderPaused = false);
        slotReceivingOrderPaused.ForEach(operatingSlot => operatingSlot.IsReceivingOrderPaused = false);
        foodIsSoldOut.ForEach(f => f.IsSoldOut = false);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            _shopRepository.UpdateRange(shopReceivingOrderPaused);
            _operatingSlotRepository.UpdateRange(slotReceivingOrderPaused);
            _foodRepository.UpdateRange(foodIsSoldOut);

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            errors.Add(e.Message);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var batchHistory = new BatchHistory
            {
                BatchCode = BatchCodes.BatchUpdateFlagReceiveOrderPauseAndSoldOut,
                Parameter = string.Empty,
                TotalRecord = shopReceivingOrderPaused.Count + slotReceivingOrderPaused.Count + foodIsSoldOut.Count,
                ErrorLog = string.Join(", ", errors),
                StartDateTime = startBatchDateTime,
                EndDateTime = endBatchDateTime,
            };

            await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            _logger.LogInformation($"Update Flag Receive Order Pause And Sold Out Batch End At: {endBatchDateTime}");
            return errors.Count > 0 ? Result.Success("Run batch fail!") : Result.Success("Run batch success!");
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}