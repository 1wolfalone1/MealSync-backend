using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Models;

public class ReviewOfShopOwnerDto
{
    public long OrderId { get; set; }

    public string Description { get; set; }

    public bool IsAllowShopReply { get; set; }

    public List<ReviewDetailDto> Reviews { get; set; }

    public class ReviewDetailDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Avatar { get; set; }

        public ReviewEntities Reviewer { get; set; }

        public RatingRanges Rating { get; set; }

        public string Comment { get; set; }

        public List<string> ImageUrls { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}