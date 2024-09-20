using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestValidateError;

public class TestValidateErrorHandler : ICommandHandler<TestValidateErrorCommand, Result>
{
    public async Task<Result<Result>> Handle(TestValidateErrorCommand request, CancellationToken cancellationToken)
    {
        return Result.Success();
    }
}