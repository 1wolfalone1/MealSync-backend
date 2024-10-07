using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Models;

public class PromotionSummaryResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? BannerUrl { get; set; }

    public string? Description { get; set; }

    public PromotionTypes Type { get; set; }

    public double? AmountRate { get; set; }

    public double? AmountValue { get; set; }

    public double MinOrdervalue { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public PromotionApplyTypes ApplyType { get; set; }
}