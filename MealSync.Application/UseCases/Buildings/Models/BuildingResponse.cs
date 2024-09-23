namespace MealSync.Application.UseCases.Buildings.Models;

public class BuildingResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public long DormitoryId { get; set; }
}