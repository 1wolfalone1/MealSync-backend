using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.FoodPackingUnits.Models;

public class FoodPackingUnitAdminResponse
{
    public long Id { get; set; }

    public long ShopId { get; set; }

    public string Name { get; set; }

    public double Weight { get; set; }

    public FoodPackingUnitType Type { get; set; }

    public int NumFoodLinked { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}