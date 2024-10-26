namespace MealSync.Application.UseCases.ShopCategories.Models;

public class ShopCategoryForShopWebResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public int NumberFoodLinked { get; set; }
}