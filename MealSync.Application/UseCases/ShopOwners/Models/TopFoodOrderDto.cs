using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopOwners.Models;

public class TopFoodOrderDto
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? ImageUrl { get; set; }

    public FoodStatus Status { get; set; }

    public int TotalOrders { get; set; }
}