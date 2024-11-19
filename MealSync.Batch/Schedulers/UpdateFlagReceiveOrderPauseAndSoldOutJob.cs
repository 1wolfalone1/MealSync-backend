using MealSync.Application.UseCases.ShopOwners.Commands.Schedulers.UpdateFlagReceiveOrderPauseAndSoldOut;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class UpdateFlagReceiveOrderPauseAndSoldOutJob : IJob
{
    private readonly IMediator _mediator;

    public UpdateFlagReceiveOrderPauseAndSoldOutJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _mediator.Send(new UpdateFlagReceiveOrderPauseAndSoldOutCommand()).ConfigureAwait(false);
    }
}