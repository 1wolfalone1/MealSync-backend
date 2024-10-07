namespace MealSync.Application.UseCases.CustomerBuildings.Models;

public class CustomerBuildingResponse
{
    public long BuildingId { get; set; }

    public string BuildingName { get; set; }

    public bool IsDefault { get; set; }
}