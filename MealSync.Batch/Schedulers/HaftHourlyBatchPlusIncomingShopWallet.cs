using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Commands.Schedulers.TransferIncomingAmountForShopAfterTwoHour;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

[DisallowConcurrentExecution]
public class HaftHourlyBatchPlusIncomingShopWallet : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HaftHourlyBatchPlusIncomingShopWallet> _logger;

    public HaftHourlyBatchPlusIncomingShopWallet(ILogger<HaftHourlyBatchPlusIncomingShopWallet> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"[BATCH CODE 3: START AT: {TimeFrameUtils.GetCurrentDate()}]");
        await _mediator.Send(new TransferIncomingAmountForShopAfterTwoHourCommand()).ConfigureAwait(false);
        _logger.LogInformation($"[BATCH CODE 3: END AT: {TimeFrameUtils.GetCurrentDate()}]");
    }
}