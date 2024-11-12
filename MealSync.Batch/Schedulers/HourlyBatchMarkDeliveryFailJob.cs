using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Commands.Schedulers;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

[DisallowConcurrentExecution]
public class HourlyBatchMarkDeliveryFailJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HourlyBatchMarkDeliveryFailJob> _logger;

    public HourlyBatchMarkDeliveryFailJob(IMediator mediator, ILogger<HourlyBatchMarkDeliveryFailJob> logger)
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