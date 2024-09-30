using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Products.Models;

public class ProductDetailResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? Description { get; set; }

    public float Price { get; set; }

    public List<CategoryResponse> Categories { get; set; }

    public List<OperatingHourResponse> OperatingHours { get; set; }

    public List<QuestionResponse> Questions { get; set; }

    public class CategoryResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class OperatingHourResponse
    {
        public long Id { get; set; }

        public long OperatingDayId { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }
    }

    public class QuestionResponse
    {
        public long Id { get; set; }

        public OptionGroupTypes Type { get; set; }

        public string Description { get; set; } = null!;

        public OptionGroupStatus Status { get; set; }

        public List<OptionResponse> Options { get; set; }
    }

    public class OptionResponse
    {
        public long Id { get; set; }

        public string Description { get; set; } = null!;

        public bool IsPricing { get; set; }

        public string? ImageUrl { get; set; }

        public float Price { get; set; }

        public OptionStatus Status { get; set; }

    }
}