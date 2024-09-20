using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestModeratorCreateLog;

public class TestModeratorLogHandler : ICommandHandler<TestModeratorLogCommand, Result>
{
    public async Task<Result<Result>> Handle(TestModeratorLogCommand request, CancellationToken cancellationToken)
    {
        return Result.Success();
    }
}