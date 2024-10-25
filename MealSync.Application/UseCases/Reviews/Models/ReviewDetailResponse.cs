using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewDetailResponse
{
    public long Id { get; set; }

    public RatingRanges Rating { get; set; }

    public string Comment { get; set; } = null!;

    public List<string> ImageUrls { get; set; }
}