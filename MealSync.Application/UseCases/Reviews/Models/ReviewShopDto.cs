using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewShopDto
{
    public long OrderId { get; set; }

    public string Description { get; set; }

    public List<ReviewDto> Reviews { get; set; }

    public class ReviewDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Avatar { get; set; }

        public ReviewEntities Reviewer { get; set; }

        public RatingRanges Rating { get; set; }

        public string Comment { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}