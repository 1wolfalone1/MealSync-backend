using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.CustomerBuildings.Commands.Update;

public class UpdateCustomerBuildingCommand : ICommand<Result>
{
    public long BuildingId { get; set; }

    public bool IsSetDefault { get; set; }
}