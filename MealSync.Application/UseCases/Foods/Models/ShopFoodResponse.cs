namespace MealSync.Application.UseCases.Foods.Models;

public class ShopFoodResponse
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public List<FoodResponse> Foods { get; set; }

    public class FoodResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string? Description { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsSoldOut { get; set; }

        public int TotalOrder { get; set; }
    }
}