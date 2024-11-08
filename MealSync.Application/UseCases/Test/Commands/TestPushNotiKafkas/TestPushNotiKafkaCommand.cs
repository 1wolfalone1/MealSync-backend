using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestPushNotiKafkas;

public class TestPushNotiKafkaCommand : ICommand<Result>
{
    public string Message { get; set; }
}