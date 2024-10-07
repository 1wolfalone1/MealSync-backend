namespace MealSync.Application.UseCases.PlatformCategory.Models;

public class PlatformCategoryResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}