using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestCloseRoom;

public class TestCloseRoomCommand : ICommand<Result>
{
    public long OrderId { get; set; }
}