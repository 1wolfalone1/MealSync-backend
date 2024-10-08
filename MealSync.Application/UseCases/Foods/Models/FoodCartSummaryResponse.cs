using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodCartSummaryResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string ImageUrl { get; set; } = null!;

    public long ShopId { get; set; }

    public FoodStatus Status { get; set; }

    public bool IsSoldOut { get; set; }
}