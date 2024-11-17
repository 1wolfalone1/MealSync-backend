using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.CommissionConfigs.Commands.Create;

public class CreateCommissionConfigCommand : ICommand<Result>
{
    public double CommissionRate { get; set; }
}