using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestOpenRoom;

public class TestOpenRoomCommand : ICommand<Result>
{
    public long OrderId { get; set; }
}