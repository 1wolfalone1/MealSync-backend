using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.Orders.Commands.Schedulers.AutoCancelEndTimeFrame;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class HalfHourlyBatchMarkOrderOverTimeFrame : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<HalfHourlyBatchMarkOrderOverTimeFrame> _logger;

    public HalfHourlyBatchMarkOrderOverTimeFrame(IMediator mediator, ILogger<HalfHourlyBatchMarkOrderOverTimeFrame> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"[BATCH CODE 7: START AT: {TimeFrameUtils.GetCurrentDate()}]");
        await _mediator.Send(new AutoCancelEndTimeFrameCommand()).ConfigureAwait(false);
        _logger.LogInformation($"[BATCH CODE 7: END AT: {TimeFrameUtils.GetCurrentDate()}]");
    }
}