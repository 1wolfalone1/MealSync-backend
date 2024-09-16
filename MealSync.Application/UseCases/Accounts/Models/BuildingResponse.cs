namespace MealSync.Application.UseCases.Accounts.Models;

public class BuildingResponse
{
    public long Id { get; set; }
    public string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
    public DormitoryResponse Dormitory { get; set; }
}