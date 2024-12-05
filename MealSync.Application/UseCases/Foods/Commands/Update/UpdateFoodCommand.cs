using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Commands.Update;

public class UpdateFoodCommand : ICommand<Result>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public double Price { get; set; }

    public string ImgUrl { get; set; } = null!;

    public FoodStatus Status { get; set; }

    public long PlatformCategoryId { get; set; }

    public long ShopCategoryId { get; set; }

    public long FoodPackingUnitId { get; set; }

    public List<long>? OperatingSlots { get; set; }

    public List<long>? FoodOptionGroups { get; set; }
}