using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class ShopOwnerFoodResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; }

    public string? ImageUrl { get; set; }

    public List<FoodResponse> Foods { get; set; } = new();

    public class FoodResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsSoldOut { get; set; }

        public int Status { get; set; }
    }
}