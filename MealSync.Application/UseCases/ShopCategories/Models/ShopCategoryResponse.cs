namespace MealSync.Application.UseCases.ShopCategories.Models;

public class ShopCategoryResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }
}