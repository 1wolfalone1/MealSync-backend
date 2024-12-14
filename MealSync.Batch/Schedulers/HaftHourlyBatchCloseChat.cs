using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Commands.Schedulers.TransferIncomingAmountForShopAfterTwoHour;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class HaftHourlyBatchCloseChat : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HaftHourlyBatchPlusIncomingShopWallet> _logger;

    public HaftHourlyBatchCloseChat(IMediator mediator, ILogger<HaftHourlyBatchPlusIncomingShopWallet> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"[BATCH CODE 8: START AT: {TimeFrameUtils.GetCurrentDate()}]");
        await _mediator.Send(new TransferIncomingAmountForShopAfterTwoHourCommand()).ConfigureAwait(false);
        _logger.LogInformation($"[BATCH CODE 8: END AT: {TimeFrameUtils.GetCurrentDate()}]");
    }
}