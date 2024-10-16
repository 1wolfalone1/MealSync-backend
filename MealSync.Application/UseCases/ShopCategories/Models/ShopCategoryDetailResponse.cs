namespace MealSync.Application.UseCases.ShopCategories.Models;

public class ShopCategoryDetailResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public List<ShopCategoryFoodResponse> Foods { get; set; }

    public class ShopCategoryFoodResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsSoldOut { get; set; }

        public int Status { get; set; }
    }
}