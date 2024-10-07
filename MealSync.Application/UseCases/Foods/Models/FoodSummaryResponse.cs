namespace MealSync.Application.UseCases.Foods.Models;

public class FoodSummaryResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }

    public string ImageUrl { get; set; } = null!;

    public long ShopId { get; set; }
}