using FluentValidation;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Test.Commands.TestModeratorCreateLog;

public class TestModeratorLogCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Email { get; set; }
}