using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Commands.Create;

public class CreateFoodCommand : ICommand<Result>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public double Price { get; set; }

    public string ImgUrl { get; set; } = null!;

    public FoodStatus Status { get; set; }

    public long PlatformCategoryId { get; set; }

    public long? ShopCategoryId { get; set; }

    public List<long> OperatingSlots { get; set; }

    public List<long>? FoodOptionGroups { get; set; }
}