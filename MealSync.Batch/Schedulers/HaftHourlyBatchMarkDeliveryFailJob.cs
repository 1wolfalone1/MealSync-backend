using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Commands.Schedulers;
using MealSync.Application.UseCases.Orders.Commands.Schedulers.OrderOverTwoHourNotDeliveryFail;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

[DisallowConcurrentExecution]
public class HaftHourlyBatchMarkDeliveryFailJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HaftHourlyBatchMarkDeliveryFailJob> _logger;

    public HaftHourlyBatchMarkDeliveryFailJob(IMediator mediator, ILogger<HaftHourlyBatchMarkDeliveryFailJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"[BATCH CODE 2: START AT: {TimeFrameUtils.GetCurrentDate()}]");
        await _mediator.Send(new OrderMarkDeliveryFailSchedulerCommand()).ConfigureAwait(false);
        _logger.LogInformation($"[BATCH CODE 2: END AT: {TimeFrameUtils.GetCurrentDate()}]");
    }
}