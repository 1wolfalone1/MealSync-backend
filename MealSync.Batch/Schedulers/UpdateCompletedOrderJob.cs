using MealSync.Application.UseCases.Orders.Commands.Schedulers.UpdateCompletedOrder;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class UpdateCompletedOrderJob : IJob
{
    private readonly IMediator _mediator;

    public UpdateCompletedOrderJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _mediator.Send(new UpdateCompletedOrderCommand()).ConfigureAwait(false);
    }
}