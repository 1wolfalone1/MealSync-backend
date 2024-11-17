using MealSync.Application.UseCases.Orders.Commands.Schedulers.UpdateStatusPendingPayment;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

[DisallowConcurrentExecution]
public class UpdateStatusPendingPaymentJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateStatusPendingPaymentJob> _logger;

    public UpdateStatusPendingPaymentJob(IMediator mediator, ILogger<UpdateStatusPendingPaymentJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _mediator.Send(new UpdateStatusPendingPaymentCommand()).ConfigureAwait(false);
    }
}