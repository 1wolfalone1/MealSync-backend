using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestFirebase;

public class TestFirebaseCommand : ICommand<Result>
{
    public string Message { get; set; }
}