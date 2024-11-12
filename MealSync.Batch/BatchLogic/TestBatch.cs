using MealSync.Application.UseCases.Orders.Commands.Schedulers;
using MediatR;

namespace MealSync.Batch.BatchLogic;

public class TestBatch : IBatchService
{
    private readonly ILogger<TestBatch> _logger;
    private readonly IMediator _mediator;

    public TestBatch(ILogger<TestBatch> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public Task ExecuteAsync()
    {
        _logger.LogInformation($"Run batch success at {DateTimeOffset.Now.Date}");
        return Task.CompletedTask;
    }
}