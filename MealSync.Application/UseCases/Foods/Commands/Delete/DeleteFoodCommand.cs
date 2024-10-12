using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Commands.Delete;

public class DeleteFoodCommand : ICommand<Result>
{
    public long Id { get; set; }
}