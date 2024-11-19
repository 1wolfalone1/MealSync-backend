using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Models;

public class ShopManageDto
{
    public long Id { get; set; }

    public string ShopName { get; set; }

    public string ShopOwnerName { get; set; }

    public string? LogoUrl { get; set; }

    public int TotalOrder { get; set; }

    public int TotalFood { get; set; }

    public double TotalRevenue { get; set; }

    public ShopStatus Status { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}