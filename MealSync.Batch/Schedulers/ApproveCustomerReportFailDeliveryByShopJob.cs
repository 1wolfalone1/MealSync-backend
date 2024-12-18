using MealSync.Application.UseCases.Reports.Commands.Schedulers.ApproveCustomerReportFailDeliveryByShop;
using MediatR;
using Quartz;

namespace MealSync.Batch.Schedulers;

public class ApproveCustomerReportFailDeliveryByShopJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApproveCustomerReportFailDeliveryByShopCommand> _logger;

    public ApproveCustomerReportFailDeliveryByShopJob(IMediator mediator, ILogger<ApproveCustomerReportFailDeliveryByShopCommand> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _mediator.Send(new ApproveCustomerReportFailDeliveryByShopCommand()).ConfigureAwait(false);
    }
}