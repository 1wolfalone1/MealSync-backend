using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.FoodPackingUnits.Models;

public class FoodPackingUnitResponse
{
    public long Id { get; set; }

    public long ShopId { get; set; }

    public string Name { get; set; }

    public double Weight { get; set; }

    public FoodPackingUnitType Type { get; set; }
}