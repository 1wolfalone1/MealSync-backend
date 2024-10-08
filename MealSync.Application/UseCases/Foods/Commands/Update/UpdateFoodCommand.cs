using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Foods.Commands.Update;

public class UpdateFoodCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public double Price { get; set; }

    public string ImgUrl { get; set; } = null!;

    public long PlatformCategoryId { get; set; }

    public long ShopCategoryId { get; set; }

    public List<long> OperatingSlots { get; set; }

    public List<long>? FoodOptionGroups { get; set; }
}