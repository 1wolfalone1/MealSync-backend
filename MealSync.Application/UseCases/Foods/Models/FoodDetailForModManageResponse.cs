using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodDetailForModManageResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int TotalOrder { get; set; }

    public FoodStatus Status { get; set; }
}